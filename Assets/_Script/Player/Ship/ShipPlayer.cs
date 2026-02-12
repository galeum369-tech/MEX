using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class ShipPlayer : MonoBehaviour
{
    ShipData shipData;

    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] Transform turret;

    [Header("🔫 기본 공격 (기관총)")]
    [SerializeField] private GameObject bulletPrefab;   // 직선 총알 프리팹
    [SerializeField] private Transform firePoint;       // 발사 위치 (총구)
    [SerializeField] private float fireRate = 0.15f;    // 연사 속도
    private bool isFiring;                              // 발사 키 누르고 있는지 여부
    private float nextFireTime;                         // 다음 발사 가능 시간

    [Header("🚀 스킬 (유도 미사일)")]
    [SerializeField] private GameObject missilePrefab;  // 유도 미사일 프리팹 (HomingMissile 붙은 거)
    [SerializeField] private float skillCooldown = 5f;  // 스킬 쿨타임
    [SerializeField] private int missileCount = 6;      // 한 번에 나가는 미사일 개수
    [SerializeField] private int missileDamage = 30;    // 미사일 데미지
    private float nextSkillTime;                        // 다음 스킬 사용 가능 시간

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

        // 물리 설정 초기화
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

            // 씬 이동 위치 처리 로직
            if (GameManager.instance.isTransitioning)
            {
                if (GameManager.instance.useRandomSpawn)
                {
                    transform.position = GameManager.instance.GetRandomSpawnPosition();
                    GameManager.instance.useRandomSpawn = false;
                }
                else
                {
                    transform.position = GameManager.instance.targetSpawnPos;
                }

                GameManager.instance.isTransitioning = false;
                Debug.Log($"📍 수송선 위치 설정 완료: {transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("GameManager 없음: 임시 데이터 사용");
            shipData = new ShipData();
        }

        inputHandler.SetControlMode(false); // TopView 모드 강제 설정
    }

    private void OnEnable()
    {
        // 이동 & 부스트
        inputHandler.OnShipMove += HandleMove;
        inputHandler.OnBoost += HandleBoost;

        // 공격 (홀드 방식)
        inputHandler.OnAttackHold += HandleAttack;

        // 스킬 (단발 방식)
        inputHandler.OnSkill += HandleSkill;
    }

    private void OnDisable()
    {
        inputHandler.OnShipMove -= HandleMove;
        inputHandler.OnBoost -= HandleBoost;
        inputHandler.OnAttackHold -= HandleAttack;
        inputHandler.OnSkill -= HandleSkill;
    }

    private void Update()
    {
        // [UI 수정] UI 열려있으면 포탑 회전이나 발사 로직 중지
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen) return;

        // 마우스 조준
        mousePos = mainCam.ScreenToWorldPoint(inputHandler.GetMousePosition());
        if (tc != null) tc.LookAt(mousePos);

        // 1. 기본 공격 로직 (누르고 있고 + 쿨타임 지남)
        if (isFiring && Time.time >= nextFireTime)
        {
            ShootBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void FixedUpdate()
    {
        // [UI 수정] UI 열려있으면 물리 이동 정지
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            rb.linearVelocity = Vector2.zero; // 멈춤
            return;
        }

        if (shipData == null) return;

        // 이동 물리 연산
        float finalSpeed = shipData.moveSpeed * (isBoosting ? shipData.boostMultiplier : 1f);
        mc.Move(currentInput, finalSpeed, shipData.acceleration);
        mc.Rotate(currentInput, shipData.turnSpeed);
    }

    // =========================================================
    // 🎮 액션 처리 함수들
    // =========================================================

    private void HandleMove(Vector2 input) => currentInput = input;
    private void HandleBoost(bool isPressed) => isBoosting = isPressed;

    // 공격 키 상태 (누름/뗌) 저장
    private void HandleAttack(bool isPressed)
    {
        // [UI 수정] UI가 열려있을 때 클릭하면 공격 대신 UI 버튼 누르기
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
        {
            if (isPressed) UIManager.Instance.ExecuteSelectedUI();
            isFiring = false; // 발사 중지
            return;
        }
        isFiring = isPressed;
    }

    // 스킬 키 눌렀을 때 실행
    private void HandleSkill()
    {
        // [UI 수정] UI 열려있으면 스킬 사용 금지
        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen) return;

        // 쿨타임 체크
        if (Time.time >= nextSkillTime)
        {
            FireHomingMissiles();
            nextSkillTime = Time.time + skillCooldown;
            Debug.Log($"🚀 미사일 발사! (다음 쿨타임: {skillCooldown}초 뒤)");
        }
        else
        {
            Debug.Log("⏳ 스킬 쿨타임 중...");
        }
    }

    // =========================================================
    // ⚔️ [수정됨] 전투 로직 (90도 보정 추가)
    // =========================================================

    // 1. 기본 사격 (직선)
    private void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // [수정] 총알 생성 시 Z축으로 90도 돌려서 생성 (오른쪽 나가는 문제 해결)
        Quaternion correctedRotation = firePoint.rotation * Quaternion.Euler(0, 0, 90f);

        Instantiate(bulletPrefab, firePoint.position, correctedRotation);
    }

    // 2. 스킬 사격 (유도 미사일 난사)
    private void FireHomingMissiles()
    {
        if (missilePrefab == null || firePoint == null) return;

        // [수정] 미사일도 마찬가지로 90도 보정
        Quaternion correctedRotation = firePoint.rotation * Quaternion.Euler(0, 0, 90f);

        for (int i = 0; i < missileCount; i++)
        {
            // 약간의 랜덤 산탄 효과 (필요 없으면 제거 가능)
            float randomSpread = Random.Range(-15f, 15f);
            Quaternion finalRotation = correctedRotation * Quaternion.Euler(0, 0, randomSpread);

            GameObject missileObj = Instantiate(missilePrefab, firePoint.position, finalRotation);

            HomingMissile missileScript = missileObj.GetComponent<HomingMissile>();
            if (missileScript != null)
            {
                missileScript.SetDamage(missileDamage);
            }
        }
    }

    // 총알이 적을 맞췄을 때 호출할 함수
    public void OnHitEnemy(float gainAmount)
    {
        shipData.currentSkillGauge = Mathf.Min(shipData.currentSkillGauge + gainAmount, shipData.maxSkillGauge);
        shipData.currentRepairGauge = Mathf.Min(shipData.currentRepairGauge + gainAmount, shipData.maxRepairGauge);

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateShipResource(
                shipData.currentSkillGauge, shipData.maxSkillGauge,
                shipData.currentRepairGauge, shipData.maxRepairGauge
            );
        }
    }
}