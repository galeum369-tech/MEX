using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MechPlayer : MonoBehaviour
{
    // --- 컴포넌트 ---
    private PlayerInputHandler inputHandler;
    private MecMoveController mc;
    private Rigidbody2D rb;
    private MechData mechData;

    // [비주얼] 주말 작업 대비용 (자식 오브젝트의 애니메이터)
    private Animator anim;

    [Header("Weapon System")]
    // ★ [추가] 현재 메카닉 손에 달려있는 무기 오브젝트 (WeaponHitbox 스크립트가 붙은 것)
    [SerializeField] private WeaponHitbox equippedHitbox;
    // ★ [추가] 현재 장착된 무기의 데이터 (ID, 데미지 등)
    [SerializeField] private MeleeWeaponData currentWeaponData;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // --- 상태 변수 ---
    private Vector2 currentInput;
    private bool isDashing;
    private bool isGrounded;
    bool isFastFalling;

    // 행동 제어 상태
    private bool isAttacking = false; // 공격 중엔 true -> 이동 불가

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        mc = new MecMoveController(rb);

        // 자식 오브젝트(Visual)에 있는 애니메이터 미리 찾기
        anim = GetComponentInChildren<Animator>();

        // 메카닉 물리 설정
        rb.gravityScale = 4f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // 데이터 연동
        if (GameManager.instance != null)
            mechData = GameManager.instance.playerData.mechData;
        else
            mechData = new MechData();

        // 핸들러 모드 설정 (사이드뷰)
        inputHandler.SetControlMode(true);
    }

    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnDash += HandleDash;
        inputHandler.OnAttack += HandleAttack;
        inputHandler.OnFastFall += HandleFastFall;
    }

    private void OnDisable()
    {
        inputHandler.OnMove -= HandleMove;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnDash -= HandleDash;
        inputHandler.OnAttack -= HandleAttack;
        inputHandler.OnFastFall -= HandleFastFall;
    }

    private void FixedUpdate()
    {
        CheckGround();

        // 1. 공격 중이면 이동 불가 (후딜 시스템)
        if (isAttacking)
        {
            mc.Stop();
            return;
        }

        // 2. 이동 처리
        float speed = isDashing ? mechData.dashSpeed : mechData.moveSpeed;

        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            mc.Move(currentInput.x, speed);
            FlipSprite(currentInput.x);
        }
        else
        {
            // ★ [수정] 빠른 하강 중이 아닐 때만 X축 정지 (하강 중에 좌우 제어 살짝 섞고 싶으면 로직 변경 가능)
            // 여기서는 깔끔하게 X축 멈춤 유지
            mc.Stop();
        }

        // ★ [추가] 빠른 하강 로직
        // "빠른 하강 키를 눌렀고" AND "땅이 아닐 때(공중일 때)"만 발동
        if (isFastFalling && !isGrounded)
        {
            mc.FastFall(mechData.fastFallSpeed);
        }
    }

    #region Callbacks

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleDash(bool isPressed) => isDashing = isPressed;

    private void HandleJump()
    {
        if (!isAttacking && isGrounded)
        {
            mc.Jump(mechData.jumpPower);
        }
    }

    private void HandleAttack()
    {
        if (isAttacking) return;

        // ★ [핵심] 무기나 데이터가 없으면 공격 불가 (에러 방지)
        if (equippedHitbox == null || currentWeaponData == null)
        {
            Debug.LogWarning("무기(Hitbox)나 데이터(Data)가 연결되지 않았습니다!");
            return;
        }

        // 1. 공격 상태 진입
        isAttacking = true;

        // 2. 히트박스에 이번 공격 스펙 주입 (데이터 사용)
        equippedHitbox.Initialize(currentWeaponData.damage, currentWeaponData.knockbackForce);

        // 3. 애니메이션 재생 (주말에 작업할 부분)
        // 지금은 애니메이션이 없으니 콜라이더가 안 켜지지만, 로직은 돌아감
        if (anim != null) anim.SetTrigger("Attack");

        Debug.Log($"⚔️ [{currentWeaponData.weaponName}] 공격 시작! (데미지: {currentWeaponData.damage})");

        // 4. 후딜 처리 (무기 데이터의 쿨타임 사용)
        // ★ 나중에 애니메이션 이벤트(MechAnimEvent) 연결하면 이 Invoke는 삭제!
        Invoke("OnAttackAnimationEnd", currentWeaponData.cooldown);
    }

    void HandleFastFall(bool isPressed)
    {
        isFastFalling = isPressed;
    }

    #endregion

    // [이벤트] 애니메이션 끝날 때(혹은 타이머 끝날 때) 호출됨
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;

        // 안전장치: 혹시라도 애니메이션이 꼬여서 콜라이더가 켜져 있을까 봐 강제로 끔
        if (equippedHitbox != null)
            equippedHitbox.GetComponent<Collider2D>().enabled = false;

        Debug.Log("✅ 공격 종료 (이동 가능)");
    }

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
        }
    }
}