using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MechPlayer : MonoBehaviour
{
    // ==================================================================================
    // [컴포넌트 및 레퍼런스]
    // ==================================================================================
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

    // ==================================================================================
    // [스킬 및 액션 설정]
    // ==================================================================================
    [Header("Plunge Attack (낙하 공격)")]
    [SerializeField] private float plungeSpeed = 30f;
    [SerializeField] private float plungeDamage = 50f;
    [SerializeField] private float plungeRadius = 3.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dodge (회피)")]
    [SerializeField] private float dodgeSpeed = 15f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 0.8f;

    // ==================================================================================
    // [상태 변수 (State Flags)]
    // ==================================================================================
    private Vector2 currentInput;
    private bool isDownPressed;

    private bool isGrounded;
    private bool wasGrounded;

    private bool isDashing;
    private bool isAttacking;
    private bool isPlunging;
    private bool isDodging;
    private bool canDodge = true;

    // [★추가됨] 안전장치 코루틴 저장용 변수
    private Coroutine failsafeCoroutine;

    // ==================================================================================
    // [초기화 및 생명주기]
    // ==================================================================================
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
            mechData = GameManager.instance.playerData.mechData;
        else
            mechData = new MechData();

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

    // ==================================================================================
    // [메인 로직 (FixedUpdate)]
    // ==================================================================================
    private void FixedUpdate()
    {
        CheckGround();

        // 1. [착지 로직]
        if (!wasGrounded && isGrounded)
        {
            if (isPlunging)
            {
                OnPlungeLand();
            }
            else if (rb.linearVelocity.y < -10f)
            {
                ac.PlayLand();
            }
        }
        wasGrounded = isGrounded;

        // 2. [행동 제한] 
        if (isAttacking || isPlunging || isDodging)
        {
            if (isPlunging && !isGrounded)
            {
                rb.linearVelocity = new Vector2(0, -plungeSpeed);
            }
            else if (isAttacking && !isDodging)
            {
                mc.Stop();
            }
            return;
        }

        // 3. [이동 처리]
        float speed = isDashing ? mechData.dashSpeed : mechData.moveSpeed;
        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            mc.Move(currentInput.x, speed);
            FlipSprite(currentInput.x);
        }
        else
        {
            mc.Stop();
        }

        // 4. [일반 빠른 하강]
        if (isDownPressed && !isGrounded && !isPlunging)
        {
            mc.FastFall(mechData.fastFallSpeed);
        }

        // 5. [애니메이션 상태 갱신]
        UpdateAnimationState();
    }

    // [★추가] 입력 끊김 방지용 타이머 변수
    private float moveStopTimer = 0f;
    private const float STOP_DELAY = 0.1f; // 0.1초 정도는 입력 없어도 봐줌

    private void UpdateAnimationState()
    {
        if (isAttacking || isPlunging || isDodging) return;

        // 1. 실제 입력 확인
        bool hasInput = Mathf.Abs(currentInput.x) > 0.01f;

        // 2. [★수정] 입력이 끊겨도 잠시동안은 움직이는 것으로 판정 (버퍼링)
        bool isMovingState = hasInput;

        if (hasInput)
        {
            moveStopTimer = 0f; // 입력이 있으면 타이머 리셋
        }
        else
        {
            // 입력이 없어도, 아주 잠깐 동안은 움직이는 상태 유지
            moveStopTimer += Time.deltaTime;
            if (moveStopTimer < STOP_DELAY)
            {
                isMovingState = true;
            }
        }

        // 3. 애니메이션 적용 (isMovingState 사용)
        // 방향 전환 시에도 Dash가 유지되도록 함
        ac.PlayMove(isMovingState);
        ac.PlayDash(isMovingState && isDashing);

        // 4. 낙하
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
        ac.PlayFall(isFalling || isPlunging);
    }

    // ==================================================================================
    // [입력 핸들러 (Callbacks)]
    // ==================================================================================
    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleDash(bool isPressed) => isDashing = isPressed;
    private void HandleDownInput(bool isPressed) => isDownPressed = isPressed;

    private void HandleJump()
    {
        if (!isAttacking && !isPlunging && !isDodging && isGrounded)
        {
            mc.Jump(mechData.jumpPower);
            ac.PlayJump();
        }
    }

    private void HandleAttack()
    {
        if (isAttacking || isPlunging || isDodging) return;

        if (!isGrounded && isDownPressed)
        {
            StartPlunge();
            return;
        }

        if (equippedHitbox == null || currentWeaponData == null)
        {
            Debug.LogWarning("무기 데이터가 없습니다.");
            return;
        }

        isAttacking = true;

        equippedHitbox.Initialize(currentWeaponData.damage, currentWeaponData.knockbackForce);
        equippedHitbox.GetComponent<Collider2D>().enabled = false;

        ac.PlayAttack();

        // [★추가됨] 안전장치 가동: 1초가 지나도 끝나지 않으면 강제로 풀어버림
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(AttackFailsafeRoutine(1.0f));
    }

    private void HandleDodge()
    {
        if (!canDodge || isDodging || !isGrounded) return;

        // [★추가됨] 회피 시작 시 혹시 켜져있을 안전장치 끄기
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);

        StartCoroutine(DodgeRoutine());
    }

    // ==================================================================================
    // [액션 로직]
    // ==================================================================================

    private IEnumerator DodgeRoutine()
    {
        isDodging = true;
        canDodge = false;

        float xDir = currentInput.x != 0 ? Mathf.Sign(currentInput.x) : transform.localScale.x;

        mc.Stop();
        rb.linearVelocity = new Vector2(xDir * dodgeSpeed, 0);

        ac.PlayDodge();

        // gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

        yield return new WaitForSeconds(dodgeDuration);

        isDodging = false;
        mc.Stop();

        // gameObject.layer = LayerMask.NameToLayer("Player");

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    private void StartPlunge()
    {
        isPlunging = true;
        isAttacking = true;

        mc.Stop();
        rb.linearVelocity = new Vector2(0, -plungeSpeed);

        Debug.Log("🚀 낙하 공격 시작!");
    }

    private void OnPlungeLand()
    {
        isPlunging = false;
        isAttacking = false;

        ac.PlayLand();

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheckPos.position, plungeRadius, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"💥 {enemy.name}에게 낙하 데미지 {plungeDamage}!");
            // enemy.GetComponent<IDamageable>()?.TakeDamage(plungeDamage);
        }
    }

    // ==================================================================================
    // [★추가됨] 안전장치 로직 (새로 추가된 부분)
    // ==================================================================================

    // 애니메이션 이벤트가 안 들어오면 강제로 상태를 푸는 타이머
    private IEnumerator AttackFailsafeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (isAttacking)
        {
            Debug.LogWarning("⚠️ 공격 상태 꼬임 감지! 강제 리셋합니다.");
            OnAttackEnd(); // 강제 종료
        }
    }

    // 외부(피격 등)에서 상태를 초기화해야 할 때 부르는 함수
    public void ResetState()
    {
        StopAllCoroutines(); // 진행 중인 회피, 안전장치 중단

        isAttacking = false;
        isPlunging = false;
        isDodging = false;
        canDodge = true;

        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;
    }

    // ==================================================================================
    // [애니메이션 이벤트 수신 함수]
    // ==================================================================================

    public void OnHitboxOpen()
    {
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = true;
    }

    public void OnHitboxClose()
    {
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;
    }

    public void OnAttackEnd()
    {
        // [★추가됨] 정상적으로 끝났으니 안전장치 타이머 해제
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);

        isAttacking = false;

        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;

        Debug.Log("✅ 공격 종료 (이벤트 수신 완료)");
    }

    public void OnPlayEffect(string effectName)
    {
        //if (ec != null) ec.PlayEffectByName(effectName);
    }

    // ==================================================================================
    // [유틸리티]
    // ==================================================================================
    private void FlipSprite(float xDir)
    {
        if (xDir > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (xDir < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void CheckGround()
    {
        if (groundCheckPos != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPos.position, plungeRadius);
        }
    }
}