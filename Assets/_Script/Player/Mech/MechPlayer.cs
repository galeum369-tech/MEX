using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MechPlayer : MonoBehaviour
{
    // ... (기존 변수들은 그대로 둠) ...
    private PlayerInputHandler inputHandler;
    private MecMoveController mc;
    private MechAnimController ac;
    private Rigidbody2D rb;
    private Animator anim;
    private MechData mechData;

    [Header("Weapon System")]
    [SerializeField] private WeaponHitbox equippedHitbox;
    [SerializeField] private MeleeWeaponData currentWeaponData;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    // ... (설정 변수들 생략 - 인스펙터 값 유지됨) ...
    [Header("Settings")]
    [SerializeField] private float plungeSpeed = 30f;
    [SerializeField] private float plungeDamage = 50f;
    [SerializeField] private float plungeRadius = 3.5f;
    [SerializeField] private float dodgeSpeed = 15f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 0.8f;

    // 상태 변수
    private Vector2 currentInput;
    private bool isDownPressed, isGrounded, wasGrounded;
    private bool isDashing, isAttacking, isPlunging, isDodging, canDodge = true;
    private Coroutine failsafeCoroutine;
    private float moveStopTimer = 0f;
    private const float STOP_DELAY = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();

        mc = new MecMoveController(rb);
        ac = new MechAnimController(anim);

        rb.gravityScale = 4f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // 1. 데이터 연결
        if (GameManager.instance != null)
        {
            mechData = GameManager.instance.playerData.mechData;

            // [★핵심] 씬 이동 위치 처리 로직
            if (GameManager.instance.isTransitioning)
            {
                if (GameManager.instance.useRandomSpawn)
                {
                    // 랜덤 스폰 (탑뷰 -> 탑뷰 랜덤일 수도 있고 등등)
                    if (SpawnPointManager.Instance != null)
                        transform.position = SpawnPointManager.Instance.GetRandomSpawnPosition();

                    GameManager.instance.useRandomSpawn = false;
                }
                else
                {
                    // 지정 위치 (포탈, 복귀 등)
                    transform.position = GameManager.instance.targetSpawnPos;
                }

                GameManager.instance.isTransitioning = false; // 이동 완료
                Debug.Log($"📍 메카 위치 설정 완료: {transform.position}");
            }
        }
        else
        {
            mechData = new MechData();
        }

        inputHandler.SetControlMode(true);
    }

    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnDash += HandleDash;
        inputHandler.OnAttack += HandleAttack;
        inputHandler.OnFastFall += HandleDownInput;
        inputHandler.OnDodge += HandleDodge;
    }

    private void OnDisable()
    {
        inputHandler.OnMove -= HandleMove;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnDash -= HandleDash;
        inputHandler.OnAttack -= HandleAttack;
        inputHandler.OnFastFall -= HandleDownInput;
        inputHandler.OnDodge -= HandleDodge;
    }

    // ... (FixedUpdate, UpdateAnimationState, 각종 핸들러 및 액션 함수들은 기존 코드와 동일하므로 생략하지 않고 그대로 유지하세요. 아래는 편의상 핵심 로직 포함) ...

    private void FixedUpdate()
    {
        CheckGround();

        if (!wasGrounded && isGrounded)
        {
            if (isPlunging) OnPlungeLand();
            else if (rb.linearVelocity.y < -10f) ac.PlayLand();
        }
        wasGrounded = isGrounded;

        if (isAttacking || isPlunging || isDodging)
        {
            if (isPlunging && !isGrounded) rb.linearVelocity = new Vector2(0, -plungeSpeed);
            else if (isAttacking && !isDodging) mc.Stop();
            return;
        }

        float speed = isDashing ? mechData.dashSpeed : mechData.moveSpeed;
        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            mc.Move(currentInput.x, speed);
            FlipSprite(currentInput.x);
        }
        else mc.Stop();

        if (isDownPressed && !isGrounded && !isPlunging) mc.FastFall(mechData.fastFallSpeed);

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (isAttacking || isPlunging || isDodging) return;
        bool hasInput = Mathf.Abs(currentInput.x) > 0.01f;
        bool isMovingState = hasInput;

        if (hasInput) moveStopTimer = 0f;
        else
        {
            moveStopTimer += Time.deltaTime;
            if (moveStopTimer < STOP_DELAY) isMovingState = true;
        }

        ac.PlayMove(isMovingState);
        ac.PlayDash(isMovingState && isDashing);
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
        ac.PlayFall(isFalling || isPlunging);
    }

    // ... (나머지 Input 핸들러 및 액션 함수들 - HandleMove, HandleJump 등 기존 코드 유지) ...
    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleDash(bool isPressed) => isDashing = isPressed;
    private void HandleDownInput(bool isPressed) => isDownPressed = isPressed;

    private void HandleJump() { if (!isAttacking && !isPlunging && !isDodging && isGrounded) { mc.Jump(mechData.jumpPower); ac.PlayJump(); } }

    private void HandleAttack()
    {
        if (isAttacking || isPlunging || isDodging) return;
        if (!isGrounded && isDownPressed) { StartPlunge(); return; }
        if (equippedHitbox == null || currentWeaponData == null) return;

        isAttacking = true;
        equippedHitbox.Initialize(currentWeaponData.damage, currentWeaponData.knockbackForce);
        equippedHitbox.GetComponent<Collider2D>().enabled = false;
        ac.PlayAttack();
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(AttackFailsafeRoutine(1.0f));
    }

    private void HandleDodge()
    {
        if (!canDodge || isDodging || !isGrounded) return;
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine()
    {
        isDodging = true; canDodge = false;
        float xDir = currentInput.x != 0 ? Mathf.Sign(currentInput.x) : transform.localScale.x;
        mc.Stop(); rb.linearVelocity = new Vector2(xDir * dodgeSpeed, 0); ac.PlayDodge();
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false; mc.Stop();
        yield return new WaitForSeconds(dodgeCooldown); canDodge = true;
    }

    private void StartPlunge() { isPlunging = true; isAttacking = true; mc.Stop(); rb.linearVelocity = new Vector2(0, -plungeSpeed); Debug.Log("🚀 낙하 공격!"); }
    private void OnPlungeLand()
    {
        isPlunging = false; isAttacking = false; ac.PlayLand();
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheckPos.position, plungeRadius, enemyLayer);
        foreach (var enemy in hitEnemies) Debug.Log($"💥 {enemy.name} 낙하 타격!");
    }

    private IEnumerator AttackFailsafeRoutine(float duration) { yield return new WaitForSeconds(duration); if (isAttacking) OnAttackEnd(); }

    public void ResetState() { StopAllCoroutines(); isAttacking = isPlunging = isDodging = false; canDodge = true; if (equippedHitbox != null) equippedHitbox.GetComponent<Collider2D>().enabled = false; }

    public void OnHitboxOpen() { if (equippedHitbox != null) equippedHitbox.GetComponent<Collider2D>().enabled = true; }
    public void OnHitboxClose() { if (equippedHitbox != null) equippedHitbox.GetComponent<Collider2D>().enabled = false; }
    public void OnAttackEnd() { if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine); isAttacking = false; if (equippedHitbox != null) equippedHitbox.GetComponent<Collider2D>().enabled = false; }
    public void OnPlayEffect(string effectName) { }

    private void FlipSprite(float xDir) { if (xDir > 0) transform.localScale = new Vector3(1, 1, 1); else if (xDir < 0) transform.localScale = new Vector3(-1, 1, 1); }
    private void CheckGround() { if (groundCheckPos != null) isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, groundCheckRadius, groundLayer); }
}