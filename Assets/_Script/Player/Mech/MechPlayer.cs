using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MechPlayer : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private MecMoveController mc;
    private MechAnimController ac;
    private Rigidbody2D rb;
    private Animator anim;
    private MechData mechData;

    // [추가] 하단 점프를 위해 현재 접촉 중인 플랫폼 저장
    private GameObject currentPlatform;

    [Header("Weapon System")]
    [SerializeField] private WeaponHitbox equippedHitbox;
    [SerializeField] private MeleeWeaponData currentWeaponData;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask platformLayer; // 인스펙터에서 플랫폼 레이어 설정 필수

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
        if (GameManager.instance != null)
        {
            mechData = GameManager.instance.playerData.mechData;

            // 씬 이동 위치 처리 로직
            if (GameManager.instance.isTransitioning)
            {
                if (GameManager.instance.useRandomSpawn)
                {
                    // [수정] SpawnPointManager 삭제됨 -> GameManager 함수 호출
                    transform.position = GameManager.instance.GetRandomSpawnPosition();
                    GameManager.instance.useRandomSpawn = false;
                }
                else
                {
                    transform.position = GameManager.instance.targetSpawnPos;
                }

                GameManager.instance.isTransitioning = false;
                Debug.Log($"📍 메카 위치 설정 완료: {transform.position}");
            }
        }
        else
        {
            // GameManager가 없을 때 (테스트용)
            mechData = new MechData();
        }

        inputHandler.SetControlMode(true); // SideView 모드
    }

    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnDash += HandleDash;
        inputHandler.OnAttack += HandleAttack;
        inputHandler.OnFastFall += HandleDownInput;
        inputHandler.OnDodge += HandleDodge; // Space 바 입력
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

    private void FixedUpdate()
    {
        // [UI 수정] UI가 열려있으면 이동 로직 정지
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            mc.Stop();
            // 움직임 애니메이션 끄기 (선택사항)
            ac.PlayMove(false);
            return;
        }

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

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleDash(bool isPressed) => isDashing = isPressed;
    private void HandleDownInput(bool isPressed) => isDownPressed = isPressed;

    // 점프는 W키로 할당됨
    private void HandleJump()
    {
        // [UI 수정] UI 열려있으면 점프 불가
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen) return;

        if (!isAttacking && !isPlunging && !isDodging && isGrounded)
        {
            mc.Jump(mechData.jumpPower);
            ac.PlayJump();
        }
    }

    // [수정] 회피(Space) 핸들러: S + Space 입력 시 하단 점프 발동
    private void HandleDodge()
    {
        // [UI 수정] UI 열려있으면 회피 불가
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen) return;

        if (!canDodge || isDodging) return;

        // S키(isDownPressed)가 눌려있고, 플랫폼 위에 있다면 하단 점프
        if (isDownPressed && isGrounded && currentPlatform != null)
        {
            StartCoroutine(DownJumpRoutine());
        }
        else if (isGrounded) // 평지나 일반 점프 중에는 회피 사용
        {
            if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
            StartCoroutine(DodgeRoutine());
        }
    }

    // 하단 점프 코루틴
    private IEnumerator DownJumpRoutine()
    {
        Collider2D platformCollider = currentPlatform.GetComponent<Collider2D>();
        CapsuleCollider2D playerCollider = GetComponent<CapsuleCollider2D>();

        if (platformCollider != null && playerCollider != null)
        {
            canDodge = false;
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

            // 아래로 가속을 주어 플랫폼을 빠르게 통과
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -5f);

            yield return new WaitForSeconds(0.3f); // 통과 대기 시간

            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            canDodge = true;
        }
    }

    private void HandleAttack()
    {
        // [UI 수정] UI가 열려있으면 공격 대신 UI 실행
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            UIManager.Instance.ExecuteSelectedUI();
            return;
        }

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

    private void FlipSprite(float xDir) { if (xDir > 0) transform.localScale = new Vector3(1, 1, 1); else if (xDir < 0) transform.localScale = new Vector3(-1, 1, 1); }

    // 적을 때렸을 때 호출할 함수
    public void OnHitEnemy(float gainAmount)
    {
        // 데이터 갱신
        mechData.currentSkillGauge = Mathf.Min(mechData.currentSkillGauge + gainAmount, mechData.maxSkillGauge);
        mechData.currentRepairGauge = Mathf.Min(mechData.currentRepairGauge + gainAmount, mechData.maxRepairGauge);

        // UI 갱신
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateMechResource(
                mechData.currentSkillGauge, mechData.maxSkillGauge,
                mechData.currentRepairGauge, mechData.maxRepairGauge
            );
        }
    }

    private void CheckGround()
    {
        if (groundCheckPos != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, groundCheckRadius, groundLayer);
    }

    // 플랫폼 감지 로직
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & platformLayer) != 0)
        {
            currentPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & platformLayer) != 0)
        {
            currentPlatform = null;
        }
    }
}