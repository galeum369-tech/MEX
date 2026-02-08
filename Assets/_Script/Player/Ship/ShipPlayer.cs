using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class ShipPlayer : MonoBehaviour
{
    ShipData shipData;

    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] Transform turret;

    private ShipMoveController mc;
    private ShipTurretController tc;
    private Rigidbody2D rb;
    private Camera mainCam;

    private Vector2 currentInput;
    private bool isBoosting;
    private Vector2 mousePos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        mc = new ShipMoveController(rb);

        if (turret != null) tc = new ShipTurretController(turret);
        mainCam = Camera.main;

        rb.gravityScale = 0f;
        rb.linearDamping = 2.0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (GameManager.instance != null)
        {
            shipData = GameManager.instance.playerData.shipData;

            // [★핵심] 씬 이동 위치 처리 로직
            if (GameManager.instance.isTransitioning)
            {
                if (GameManager.instance.useRandomSpawn)
                {
                    // 랜덤 스폰 (탑뷰 씬 진입 시)
                    if (SpawnPointManager.Instance != null)
                        transform.position = SpawnPointManager.Instance.GetRandomSpawnPosition();

                    GameManager.instance.useRandomSpawn = false;
                }
                else
                {
                    // 지정 위치
                    transform.position = GameManager.instance.targetSpawnPos;
                }

                GameManager.instance.isTransitioning = false; // 이동 완료
                Debug.Log($"📍 수송선 위치 설정 완료: {transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("GameManager 없음: 임시 데이터 사용");
            shipData = new ShipData();
        }

        inputHandler.SetControlMode(false); // TopView 모드
    }

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
        // GetMousePosition()이 이제 하드웨어 좌표를 주므로 문제 없음
        mousePos = mainCam.ScreenToWorldPoint(inputHandler.GetMousePosition());

        if (tc != null) tc.LookAt(mousePos);
    }

    private void FixedUpdate()
    {
        if (shipData == null) return;
        float finalSpeed = shipData.moveSpeed * (isBoosting ? shipData.boostMultiplier : 1f);
        mc.Move(currentInput, finalSpeed, shipData.acceleration);
        mc.Rotate(currentInput, shipData.turnSpeed);
    }

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleBoost(bool isPressed) => isBoosting = isPressed;
}