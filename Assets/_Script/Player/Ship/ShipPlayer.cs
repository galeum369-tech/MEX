using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class ShipPlayer : MonoBehaviour
{
    ShipData shipData;

    [SerializeField] private PlayerInputHandler inputHandler;

    //포탑 오브젝트 연결
    [SerializeField] Transform turret;

    private ShipMoveController mc;
    private ShipTurretController tc;
    private Rigidbody2D rb;
    private Camera mainCam;     //마우스 좌표 변환용

    // 상태 변수
    private Vector2 currentInput;
    private bool isBoosting;
    private Vector2 mousePos;   //마우스 월드 좌표

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        mc = new ShipMoveController(rb);

        if(turret != null)
            tc = new ShipTurretController(turret);

        mainCam = Camera.main;

        // 물리 세팅
        rb.gravityScale = 0f;
        rb.linearDamping = 2.0f; // 마찰력 (키 뗄 때 감속용)
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        // 게임 매니저에서 플레이어 데이터 가져오기
        if (GameManager.instance != null)
        {
            shipData = GameManager.instance.playerData.shipData;
        }
        else
        {
            Debug.LogWarning("GameManager가 없음 임시 데이터 사용");
            shipData = new ShipData(); // 기본값 사용
        }

        //인풋 핸들러에게 탑뷰임을 알림
        inputHandler.SetControlMode(false); // false = 탑뷰
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

    private void Update()
    {
        //마우스 좌표를 월드 좌표로 변환
        mousePos = mainCam.ScreenToWorldPoint(inputHandler.GetMousePosition());

        if(tc != null)
            tc.LookAt(mousePos);
    }

    private void FixedUpdate()
    {
        if(shipData == null) return; // 데이터 없으면 무시

        float finalSpeed = shipData.moveSpeed * (isBoosting ? shipData.boostMultiplier : 1f);

        // 가속도(acceleration) 파라미터 추가됨
        mc.Move(currentInput, finalSpeed, shipData.acceleration);

        // 회전
        mc.Rotate(currentInput, shipData.turnSpeed);
    }

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleBoost(bool isPressed) => isBoosting = isPressed;
}
