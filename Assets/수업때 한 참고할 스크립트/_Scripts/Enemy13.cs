using UnityEngine;

public class Enemy13 : MonoBehaviour, IDamageable13
{
    // 컨트롤러
    AnimController13 ac;
    MoveController13 mc;
    AttackSystem13 attSystem;

    // 컴포넌트
    Animator anim;
    Rigidbody2D rb;

    // 적 속성
    [SerializeField] Transform targetPlayer;
    [SerializeField] int hp = 100;                   // 체력
    [SerializeField] float moveSpeed = 2f;           // 이동속도
    [SerializeField] float attackRange = 1f;         // 공격범위
    [SerializeField] float attackCoolTime = 2f;      // 공격쿨타임
    [SerializeField] float chaseRange = 3f;          // 추적범위

    [SerializeField] Transform attPoint;
    [SerializeField] float attRadius = 0.5f;
    [SerializeField] int attDamage = 10;
    [SerializeField] LayerMask playerLayer;

    // FSM 상태 정의
    enum State
    {
        Idle,           // 대기 : 플레이어가 추적범위 밖에 있을때
        Chase,          // 추적 : 플레이어가 추적범위 안에 있을때
        Attack,         // 공격 : 플레이어가 공격범위 안에 있을때
        Hit,            // 피격 : 플레이어에게 피격당했을때
        Die             // 사망 : 체력이 0이 되었을때
    }

    // 현재 상태 (기본값 : Idle)
    State currentState = State.Idle;

    // 마지막 공격한 시간 (Time.time)
    float lastAttackTime;

    // 사망 여부
    bool isDead = false;

    // 최적화 시키기 위한 변수 (거리 계산용)
    float chaseRangeSqr;
    float attackRangeSqr;
    float distanceSqr;
    Vector2 directionToPlayer;


    private void Awake()
    {
        // 컴포넌트 할당
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 컨트롤러 할당
        mc = new MoveController13(rb);
        ac = new AnimController13(anim);
        attSystem = new AttackSystem13(attPoint, attRadius, attDamage, playerLayer);

        // 거리 제곱값 계산
        chaseRangeSqr = chaseRange * chaseRange;
        attackRangeSqr = attackRange * attackRange;

    }

    void Update()
    {
        if (isDead || targetPlayer == null) return;

        // FSM 상태 처리
        UpdateState();
        UpdateStateWithDistance();
    }

    // 거리에 따른 상태 처리
    private void UpdateStateWithDistance()
    {
        Vector2 dir = targetPlayer.position - transform.position;
        directionToPlayer = dir.normalized;
        distanceSqr = dir.sqrMagnitude;

        if (distanceSqr > chaseRangeSqr) //플레이어가 추적 범위 밖에 있음
        {
            currentState = State.Idle;
        }
        else if (distanceSqr <= attackRangeSqr) //플레이어가 공격 범위 안에 있음
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Chase;
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Chase:
                HandleChaseState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
        }
    }

    /// <summary>
    /// 플레이어가 추적 범위에 들어오면 추적 상태로 전환
    /// </summary>
    /// <param name="distance"></param>
    void HandleIdleState()
    {
        mc.Stop();
        ac.PlayMove(false);
    }

    /// <summary>
    /// 플레이어를 추적하고, 범위에 따라 상태 전환
    /// </summary>
    /// <param name="distance"></param>
    void HandleChaseState()
    {
        mc.Move(directionToPlayer, moveSpeed);
        ac.PlayMove(true);
        FilpDirectionX();
    }

    /// <summary>
    /// 플레이어 공격, 범위에 따라 상태 전환
    /// </summary>
    /// <param name="distance"></param>
    void HandleAttackState()
    {
        // 공격범위에 있을때 이동정지
        mc.Stop();
        ac.PlayMove(false);

        // 플레이어 방향으로 회전
        FilpDirectionX();

        // 공격 쿨타임 체크
        if (Time.time >= lastAttackTime + attackCoolTime)
        {
            ac.PlayAttack();
            lastAttackTime = Time.time;
        }
    }

    void FilpDirectionX()
    {
        if (directionToPlayer.x != 0)
        {
            transform.localScale = new Vector3(directionToPlayer.x < 0 ? -1 : 1, 1, 1);
        }
    }

    public void AnimEvent()
    {
        attSystem.AnimEvent_CheckCollier();
    }
    public void TakeDamage(int damage)
    {
        // 이미 죽었으면 무시
        if (isDead) return;

        hp = Mathf.Max(0, hp - damage);
        if (hp <= 0) Die();
        else Hit();
    }

    /// <summary>
    /// 피격 상태 처리, 거리 따라 상태 전환
    /// </summary>
    void Hit()
    {
        currentState = State.Hit;
        mc.Stop();
        ac.PlayHit();
    }

    void Die()
    {
        isDead = true;
        currentState = State.Die;
        mc.Stop();
        ac.PlayDie();
        Destroy(gameObject, 2f);
    }

    [ContextMenu("Hit Test")]
    void HitTest()
    {
        TakeDamage(20);
    }

    private void OnDrawGizmos()
    {
        // 추적 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        // 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
