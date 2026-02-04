using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 필요

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MechPlayer : MonoBehaviour
{
    // ==================================================================================
    // [컴포넌트 및 레퍼런스]
    // ==================================================================================
    private PlayerInputHandler inputHandler;
    private MecMoveController mc;   // 물리 이동 처리
    private MechAnimController ac;  // 애니메이션 처리
    //private MechEffectController ec; // 이펙트 처리 (옵션)

    private Rigidbody2D rb;
    private Animator anim;
    private MechData mechData;

    [Header("Weapon System")]
    [SerializeField] private WeaponHitbox equippedHitbox;     // 실제 무기 오브젝트 (Collider 포함)
    [SerializeField] private MeleeWeaponData currentWeaponData; // 데미지 등 데이터

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // ==================================================================================
    // [스킬 및 액션 설정]
    // ==================================================================================
    [Header("Plunge Attack (낙하 공격)")]
    [SerializeField] private float plungeSpeed = 30f;     // 낙하 속도
    [SerializeField] private float plungeDamage = 50f;    // 착지 데미지
    [SerializeField] private float plungeRadius = 3.5f;   // 충격파 범위
    [SerializeField] private LayerMask enemyLayer;        // 데미지 대상 레이어

    [Header("Dodge (회피)")]
    [SerializeField] private float dodgeSpeed = 15f;      // 회피 이동 속도
    [SerializeField] private float dodgeDuration = 0.4f;  // 회피 지속 시간
    [SerializeField] private float dodgeCooldown = 0.8f;  // 쿨타임

    // ==================================================================================
    // [상태 변수 (State Flags)]
    // ==================================================================================
    // 입력 상태
    private Vector2 currentInput;
    private bool isDownPressed;    // S키(아래) 입력 중인가?

    // 물리 상태
    private bool isGrounded;
    private bool wasGrounded;      // 착지 순간 감지용

    // 행동 상태
    private bool isDashing;
    private bool isAttacking;
    private bool isPlunging;       // 낙하 공격 중인가?
    private bool isDodging;
    private bool canDodge = true;

    // ==================================================================================
    // [초기화 및 생명주기]
    // ==================================================================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();

        // 컨트롤러 클래스 초기화
        mc = new MecMoveController(rb);
        ac = new MechAnimController(anim);
       // ec = GetComponent<MechEffectController>(); // 같은 객체에 있다고 가정 (없으면 null 처리됨)

        // 물리 엔진 설정
        rb.gravityScale = 4f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // 데이터 매니저에서 스탯 가져오기 (없으면 기본값)
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
        inputHandler.OnFastFall += HandleDownInput; // S키 입력
        inputHandler.OnDodge += HandleDodge;        // 회피 입력
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

        // 1. [착지 로직] 공중 -> 땅 닿는 순간
        if (!wasGrounded && isGrounded)
        {
            if (isPlunging)
            {
                OnPlungeLand(); // 낙하 공격 착지 (데미지 + 이펙트)
            }
            else if (rb.linearVelocity.y < -10f) // 일반 고공 낙하
            {
                ac.PlayLand();       // 착지 모션
               // if (ec) ec.PlayLandEffect();
            }
        }
        wasGrounded = isGrounded; // 상태 저장

        // 2. [행동 제한] 공격, 찍기, 회피 중엔 일반 이동 불가
        if (isAttacking || isPlunging || isDodging)
        {
            if (isPlunging && !isGrounded)
            {
                // 낙하 공격 중엔 수직 가속 유지 (공중 멈춤 방지)
                rb.linearVelocity = new Vector2(0, -plungeSpeed);
            }
            else if (isAttacking && !isDodging)
            {
                // 지상 공격 중엔 제자리 정지 (미끄러짐 방지)
                mc.Stop();
            }
            // 회피(Dodge)는 코루틴에서 속도를 제어하므로 건드리지 않음
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

        // 4. [일반 빠른 하강] (낙하 공격 아닐 때만)
        if (isDownPressed && !isGrounded && !isPlunging)
        {
            mc.FastFall(mechData.fastFallSpeed);
        }

        // 5. [애니메이션 상태 갱신]
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 이동: 입력 있음 + 회피 중 아님
        bool isMoving = Mathf.Abs(currentInput.x) > 0.01f;
        ac.PlayMove(isMoving && !isDodging);

        // 낙하: 땅 아님 + 속도 아래로 (낙하 공격 중일 때도 Fall 모션 유지)
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
        // 공격, 찍기, 회피 중 점프 불가
        if (!isAttacking && !isPlunging && !isDodging && isGrounded)
        {
            mc.Jump(mechData.jumpPower);
            ac.PlayJump(); // Jump Start
           // if (ec) ec.PlayJumpDust();
        }
    }

    private void HandleAttack()
    {
        if (isAttacking || isPlunging || isDodging) return;

        // [낙하 공격 발동] 공중 + S키
        if (!isGrounded && isDownPressed)
        {
            StartPlunge();
            return;
        }

        // [일반 지상 공격]
        if (equippedHitbox == null || currentWeaponData == null)
        {
            Debug.LogWarning("무기 데이터가 없습니다.");
            return;
        }

        isAttacking = true;

        // 데이터 초기화 (히트박스는 끄고 대기 -> 애니메이션 이벤트가 켬)
        equippedHitbox.Initialize(currentWeaponData.damage, currentWeaponData.knockbackForce);
        equippedHitbox.GetComponent<Collider2D>().enabled = false;

        ac.PlayAttack(); // 애니메이션 재생

        // ★ 중요: Invoke 삭제됨. 애니메이션 이벤트가 끝을 알려줄 것임.
    }

    private void HandleDodge()
    {
        if (!canDodge || isDodging || !isGrounded) return; // (필요 시 공중 회피 허용 가능)
        StartCoroutine(DodgeRoutine());
    }

    // ==================================================================================
    // [액션 로직 (Action Logic)]
    // ==================================================================================

    // --- 회피 코루틴 ---
    private IEnumerator DodgeRoutine()
    {
        isDodging = true;
        canDodge = false;

        // 방향 결정 (입력 없으면 보는 방향)
        float xDir = currentInput.x != 0 ? Mathf.Sign(currentInput.x) : transform.localScale.x;

        // 순간 가속
        mc.Stop();
        rb.linearVelocity = new Vector2(xDir * dodgeSpeed, 0);

        ac.PlayDodge(); // 회피 애니메이션 Trigger

        // TODO: 여기서 플레이어 무적 레이어로 변경 (Layer Collision Matrix 설정 필요)
        // gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

        yield return new WaitForSeconds(dodgeDuration);

        isDodging = false;
        mc.Stop(); // 미끄러짐 방지

        // TODO: 레이어 복구
        // gameObject.layer = LayerMask.NameToLayer("Player");

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    // --- 낙하 공격 시작 ---
    private void StartPlunge()
    {
        isPlunging = true;
        isAttacking = true;

        // 공중 멈춤 후 급강하
        mc.Stop();
        rb.linearVelocity = new Vector2(0, -plungeSpeed);

        // 필요 시 전용 사운드 재생
        Debug.Log("🚀 낙하 공격 시작!");
    }

    // --- 낙하 공격 착지 (FixedUpdate에서 호출) ---
    private void OnPlungeLand()
    {
        isPlunging = false;
        isAttacking = false;

        ac.PlayLand(); // 착지 애니메이션
        //if (ec) ec.PlayPlungeImpact(); // 쾅! 이펙트

        // [히트박스 처리] 범위 공격 (무기 Collider 아님)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheckPos.position, plungeRadius, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"💥 {enemy.name}에게 낙하 데미지 {plungeDamage}!");
            // enemy.GetComponent<IDamageable>()?.TakeDamage(plungeDamage);
        }
    }

    // ==================================================================================
    // [★ 애니메이션 이벤트 수신 함수 (Animator에서 호출)]
    // ==================================================================================

    // 1. 공격 판정 켜기 (휘두르는 프레임)
    public void OnHitboxOpen()
    {
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = true;
    }

    // 2. 공격 판정 끄기 (동작 끝나는 프레임)
    public void OnHitboxClose()
    {
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;
    }

    // 3. 공격 행동 완전 종료 (후딜 종료 프레임)
    public void OnAttackEnd()
    {
        isAttacking = false;
        // 안전장치로 한 번 더 끔
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;

        Debug.Log("✅ 공격 종료 (이벤트 수신 완료)");
    }

    // 4. 이펙트 재생 (문자열로 이름 받기)
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

            // 낙하 공격 범위 미리보기
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPos.position, plungeRadius);
        }
    }
}