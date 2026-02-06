using System.Collections;
using UnityEngine;

public class SupportDrone : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;       // 따라다닐 플레이어
    [SerializeField] private PlayerInputHandler inputHandler; // 우클릭 입력 감지용

    // ========================================================================
    // [2. 이동 관련 변수 (생략 없음)]
    // ========================================================================
    [Header("Movement Settings")]
    [Tooltip("플레이어 기준 드론의 위치 (x: 뒤쪽 거리, y: 높이)")]
    public Vector3 offset = new Vector3(-1.5f, 2.0f, 0);

    [Tooltip("따라오는 속도 (낮을수록 빠릿함, 높을수록 부드럽게 지연됨)")]
    public float smoothTime = 0.2f;

    // SmoothDamp 함수가 내부적으로 사용하는 속도 참조 변수 (건드리지 마세요)
    private Vector3 currentVelocity;

    // 드론이 현재 오른쪽을 보고 있는가?
    private bool isFacingRight = true;

    // ========================================================================
    // [3. 전투/스킬 관련 변수]
    // ========================================================================
    [Header("Skill Gauge Settings")]
    public float maxGauge = 100f;        // 게이지 최대치
    public float currentGauge = 0f;      // 현재 게이지
    public float gainPerHit = 10f;       // 타격당 차오르는 양

    [Header("Missile Settings")]
    public GameObject missilePrefab;     // 발사할 유도 미사일 프리팹
    public Transform firePoint;          // 미사일이 생성될 총구 위치 (드론의 자식 오브젝트)

    [Header("Assist (Passive)")]
    public float assistCooldown = 0.2f;  // 추가타 내부 쿨타임 (너무 많이 나가는 것 방지)
    private float lastAssistTime;

    [Header("Ultimate (Active)")]
    public int ultMissileCount = 40;     // 우클릭 시 발사할 미사일 총 개수
    public float ultFireRate = 0.05f;    // 우클릭 시 발사 간격 (빠르게)

    // 현재 전탄 발사 스킬을 쓰고 있는 중인지 확인 (중복 실행 방지)
    private bool isFiringSkill = false;


    // ========================================================================
    // [초기화 및 이벤트 연결]
    // ========================================================================
    private void Start()
    {
        // 플레이어를 안 넣었다면 태그로 자동 찾기
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        // 인풋 핸들러 자동 찾기
        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        // 우클릭(Skill) 입력 이벤트 연결
        if (inputHandler != null) inputHandler.OnSkill += TryUseSkill;
    }

    private void OnDisable()
    {
        if (inputHandler != null) inputHandler.OnSkill -= TryUseSkill;
    }

    // ========================================================================
    // [메인 루프]
    // ========================================================================
    // 카메라는 LateUpdate, 쫄래쫄래 따라다니는 펫/드론도 LateUpdate가 국룰 (떨림 방지)
    private void LateUpdate()
    {
        FollowPlayer();
    }

    // ========================================================================
    // [기능 1] 이동 로직 (Follow Player)
    // ========================================================================
    private void FollowPlayer()
    {
        if (playerTransform == null) return;

        // 1. 플레이어가 어느 방향을 보고 있는지 확인 (Scale.x 이용)
        // (플레이어가 오른쪽(1)을 보면 1, 왼쪽(-1)을 보면 -1)
        float playerDir = Mathf.Sign(playerTransform.localScale.x);

        // 2. 목표 위치 계산
        // 플레이어 등 뒤에 위치해야 하므로 offset.x에 방향을 곱해줌
        // 예: 오른쪽 볼 땐 (-1.5, 2), 왼쪽 볼 땐 (1.5, 2)
        Vector3 targetPos = playerTransform.position + new Vector3(offset.x * playerDir, offset.y, 0);

        // 3. 부드러운 이동 (SmoothDamp)
        // transform.position을 targetPos로 부드럽게 이동시킴
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        // 4. 드론 회전 (플레이어와 같은 곳을 보게 함)
        if (playerDir > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (playerDir < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // 좌우 반전
        transform.localScale = scale;
    }

    // ========================================================================
    // [기능 2] 패시브 공격 (플레이어가 적을 때릴 때 호출)
    // ========================================================================
    public void OnPlayerHitEnemy(Vector2 enemyPos)
    {
        // 스킬 사용 중엔 패시브 발동 안 함 (선택사항)
        if (isFiringSkill) return;

        // 1. 게이지 충전
        if (currentGauge < maxGauge)
        {
            currentGauge += gainPerHit;

            // 게이지 꽉 참 처리
            if (currentGauge >= maxGauge)
            {
                currentGauge = maxGauge;
                // Debug.Log("⚡ 드론: 발사 준비 완료!"); 
            }
        }

        // 2. 추가타 미사일 1발 발사 (쿨타임 있음)
        if (Time.time >= lastAssistTime + assistCooldown)
        {
            lastAssistTime = Time.time;
            FireSingleMissile(); // 1발 슝!
        }
    }

    // ========================================================================
    // [기능 3] 액티브 스킬 (우클릭 시 전탄 발사)
    // ========================================================================
    private void TryUseSkill()
    {
        if (isFiringSkill) return; // 이미 쏘는 중이면 무시

        // 게이지가 꽉 찼는지 확인
        if (currentGauge >= maxGauge)
        {
            StartCoroutine(FireMissileBarrage());
        }
        else
        {
            // Debug.Log("❌ 게이지 부족!");
        }
    }

    private IEnumerator FireMissileBarrage()
    {
        isFiringSkill = true;
        currentGauge = 0f; // 게이지 소모
        Debug.Log("🚀 [드론] 전탄 발사!!");

        // 다다다다 발사
        for (int i = 0; i < ultMissileCount; i++)
        {
            FireSingleMissile();
            yield return new WaitForSeconds(ultFireRate);
        }

        isFiringSkill = false;
    }

    // ========================================================================
    // [유틸리티] 미사일 생성
    // ========================================================================
    private void FireSingleMissile()
    {
        if (missilePrefab != null && firePoint != null)
        {
            Instantiate(missilePrefab, firePoint.position, firePoint.rotation);
        }
    }
}