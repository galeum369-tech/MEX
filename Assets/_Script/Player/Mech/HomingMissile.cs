using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 리지드바디 자동 추가
public class HomingMissile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;           // 날아가는 속도
    public float rotateSpeed = 250f;    // 회전 속도 (클수록 적을 칼같이 쫓아감)
    public float detectionRadius = 20f; // 적 탐지 범위
    public float lifetime = 3f;         // n초 동안 못 맞추면 자폭

    [Header("Targeting")]
    public LayerMask enemyLayer;        // 적 레이어 (★필수 설정)
    public LayerMask wallLayer;         // 벽/지형 레이어
    [Header("Combat")]
    public int damage = 10;             // 기본 데미지 (드론이 SetDamage로 덮어씌움)

    private Rigidbody2D rb;
    private Transform target;           // 쫓아갈 놈
    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // 1. 수명 초기화
        lifeTimer = lifetime;

        // 2. 태어나자마자 가장 가까운 적 찾기
        target = FindClosestEnemy();

        // 3. [연출] 처음 발사될 때 약간 랜덤한 방향으로 퍼지게 (산탄 느낌)
        // -30도 ~ 30도 사이로 랜덤 회전해서 나감
        float randomSpread = Random.Range(-30f, 30f);
        transform.Rotate(0, 0, randomSpread);
    }

    // [★추가] 외부(드론)에서 이 미사일의 데미지를 설정하는 함수
    public void SetDamage(int newDamage)
    {
        this.damage = newDamage;
    }

    private void FixedUpdate()
    {
        // 1. 앞으로 전진 (무조건 자신의 오른쪽 방향으로)
        rb.linearVelocity = transform.right * speed;

        // 2. 유도 로직 (타겟이 있으면 그쪽으로 회전)
        if (target != null)
        {
            // 타겟 방향 벡터 계산
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            // 외적(Cross Product)을 이용해 타겟이 내 왼쪽에 있는지 오른쪽에 있는지 판별
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            // 회전 적용 (rotateAmount가 양수면 시계방향, 음수면 반시계)
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else
        {
            // 타겟이 없으면 회전 멈춤 (직진)
            rb.angularVelocity = 0f;

            // (선택 사항) 날아가는 도중에 타겟을 잃으면 새로운 적을 찾을지 여부
            // target = FindClosestEnemy(); 
        }

        // 3. 수명 체크
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0)
        {
            DestroyMissile();
        }
    }

    // 가장 가까운 적 찾는 함수
    private Transform FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }
        return closest;
    }

    // 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        int targetLayer = (1 << collision.gameObject.layer);
        // 1. 적과 부딪혔는지 확인 (LayerMask 연산)
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // [★적용] 적에게 실제 데미지 전달
            // 적 스크립트에 'TakeDamage' 같은 함수가 있다고 가정하고 호출
            // (인터페이스 IDamageable을 쓴다면 아래 주석 해제)

            
            IDamageable targetStats = collision.GetComponent<IDamageable>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage, damage);
            }
            

            // 혹은 Enemy 스크립트를 직접 참조하는 경우:
            // Enemy enemy = collision.GetComponent<Enemy>();
            // if (enemy != null) enemy.TakeDamage(damage);

            Debug.Log($"💥 명중! {collision.name}에게 {damage} 데미지!");

            // 타격 이펙트 생성 (필요시)
            // Instantiate(hitEffect, transform.position, Quaternion.identity);

            DestroyMissile(); // 명중했으니 삭제
        }
        // 2. 벽(Ground)이나 장애물에 부딪혔을 때
        else if ((targetLayer & wallLayer) != 0)
        {
            // 벽에 박혔을 때 연기 이펙트 같은 거 넣기 좋음
            DestroyMissile();
        }
    }

    private void DestroyMissile()
    {
        // 나중에 오브젝트 풀링을 쓴다면 Destroy 대신 gameObject.SetActive(false); 로 변경
        Destroy(gameObject);
    }
}