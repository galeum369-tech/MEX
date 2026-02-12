using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class HubPlayer : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private HubMoveController mc;
    private Rigidbody2D rb;

    [Header("허브 플레이어 스탯")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float fastFallSpeed = 15f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPos; // 발밑에 빈 오브젝트 배치 후 연결
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;    // 바닥 레이어 설정

    private Vector2 currentInput;
    private bool isGrounded;
    private bool isDownPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        mc = new HubMoveController(rb); // 분리형 컨트롤러 유지

        // 물리 설정
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (inputHandler != null)
            inputHandler.SetControlMode(true); // SideView 모드
    }

    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnFastFall += HandleFastFall;
        inputHandler.OnInteract += HandleInteract;
    }

    private void OnDisable()
    {
        inputHandler.OnMove -= HandleMove;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnFastFall -= HandleFastFall;
        inputHandler.OnInteract -= HandleInteract;
    }

    private void Update()
    {
        // 땅 체크 (Gizmos로 범위 확인 가능)
        if (groundCheckPos != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, groundCheckRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        // UI가 열려있으면 이동 금지
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            mc.Stop();
            return;
        }

        // 이동 로직
        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            mc.Move(currentInput.x, moveSpeed);
            FlipDirectionX(currentInput.x);
        }
        else
        {
            mc.Stop(); // 키 떼면 바로 멈춤
        }

        // 빠른 하강
        if (isDownPressed && !isGrounded)
        {
            mc.FastFall(fastFallSpeed);
        }
    }

    #region 이벤트 콜백
    private void HandleMove(Vector2 input) => currentInput = input;

    private void HandleJump()
    {
        // UI 열림 체크 & 땅 체크
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen) return;

        if (isGrounded)
        {
            mc.Jump(jumpForce);
        }
    }

    private void HandleFastFall(bool isPressed) => isDownPressed = isPressed;

    private void HandleInteract()
    {
        // UI 조작용
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            UIManager.Instance.ExecuteSelectedUI();
            return;
        }
        Debug.Log("Hub Interact!");
    }
    #endregion

    void FlipDirectionX(float xDir)
    {
        if (xDir > 0) transform.localScale = Vector3.one;
        else if (xDir < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // 에디터에서 범위 확인용
    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);
        }
    }
}