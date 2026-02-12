using UnityEngine;

public enum EnemyState { Idle, Chase, Attack, Special, Hurt, Die }

public class EnemyMelee : EnemyBase
{
    private EnemyState currentState = EnemyState.Idle;
    private Transform target;
    private float actionTimer = 0f;

    public override void Init(int difficulty)
    {
        base.Init(difficulty);
        // 플레이어 태그로 타겟 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void Update()
    {
        if (currentState == EnemyState.Die || currentState == EnemyState.Hurt) return;

        if (actionTimer > 0) actionTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Chase: UpdateChase(); break;
            case EnemyState.Attack: // 공격 중 로직은 애니메이션 이벤트로 처리 권장
            case EnemyState.Special: break;
        }
    }

    private void UpdateIdle()
    {
        ac.PlayMove(false);
        // 인식 거리 안에 들어오면 추격 시작
        if (target != null && Vector2.Distance(transform.position, target.position) < data.detectionRange)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void UpdateChase()
    {
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        float dir = target.position.x > transform.position.x ? 1 : -1;

        // 1. 공격 범위 체크
        if (dist <= data.attackRange && actionTimer <= 0)
        {
            DetermineNextAction(); // 일반 공격 혹은 스페셜 스킬 결정
            return;
        }

        // 2. 이동 및 단차 극복
        mc.Move(dir, currentMoveSpeed);
        ac.PlayMove(true);

        if (mc.CheckStep(lowerRayPoint, upperRayPoint, dir, 0.7f, groundLayer))
        {
            mc.Jump(8f); // 레이캐스트 기반 단차 점프
        }
    }

    private void DetermineNextAction()
    {
        mc.Stop();

        // 엘리트이고 확률(예: 30%)에 걸리면 스페셜 스킬 사용
        if (data.isElite && Random.value < 0.3f)
        {
            ExecuteSpecial();
        }
        else
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        currentState = EnemyState.Attack;
        ac.PlayAttack(); // 일반 공격 모션 1개
        actionTimer = data.attackCooldown;
    }

    private void ExecuteSpecial()
    {
        currentState = EnemyState.Special;
        ac.PlaySpecial(); // 엘리트 전용 스페셜 스킬
        actionTimer = data.attackCooldown * 1.5f; // 스킬은 쿨타임 더 길게
    }

    // 애니메이션 이벤트에서 호출 (공격 끝날 때)
    public void OnActionEnd()
    {
        currentState = EnemyState.Chase;
    }
}