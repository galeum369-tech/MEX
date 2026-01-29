using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipPlayer : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputHandler inputHandler;

    private ShipMoveController mc;
    private Rigidbody2D rb;

    [System.Serializable]
    public class ShipStat
    {
        [Header("기동성")]
        public float moveSpeed = 10f;       // 최고 속도
        public float boostMultiplier = 1.5f;// 부스터 배율
        [Range(1f, 50f)]
        public float acceleration = 5f;     // 가속력 (반응성) - 5 추천
        public float turnSpeed = 200f;      // 선회력 - 200 추천
    }

    [Header("Ship Settings")]
    public ShipStat stat; // 인스펙터에서 수정 가능

    // 상태 변수
    private Vector2 currentInput;
    private bool isBoosting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        mc = new ShipMoveController(rb);

        // [물리 세팅: 브루저 타입]
        rb.gravityScale = 0f;
        rb.linearDamping = 2.0f; // 마찰력 (키 뗄 때 감속용)
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // (OnEnable, OnDisable은 이전과 동일)
    private void OnEnable()
    {
        inputHandler.OnShipMove += HandleMove;
        inputHandler.OnBoost += HandleBoost;
    }
    private void OnDisable()
    {
        inputHandler.OnShipMove -= HandleMove;
        inputHandler.OnBoost -= HandleBoost;
    }

    private void FixedUpdate()
    {
        float finalSpeed = stat.moveSpeed * (isBoosting ? stat.boostMultiplier : 1f);

        // 가속도(acceleration) 파라미터 추가됨
        mc.Move(currentInput, finalSpeed, stat.acceleration);

        // 회전
        mc.Rotate(currentInput, stat.turnSpeed);
    }

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleBoost(bool isPressed) => isBoosting = isPressed;
}
