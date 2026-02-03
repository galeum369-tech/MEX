using UnityEngine;

public class AnimController13
{
    // 애니메이터 컴포넌트
    Animator animator;

    // 애니메이션 해시값
    private readonly int hashMove = Animator.StringToHash("IsMove");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");

    // 생성자 (DI)
    public AnimController13(Animator animator)
    {
        this.animator = animator;
    }

    public void PlayMove(bool isMoving)
    {
        animator.SetBool(hashMove, isMoving);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(hashAttack);
    }

    public void PlayHit()
    {
        animator.SetTrigger(hashHit);
    }

    public void PlayDie()
    {
        animator.SetTrigger(hashDie);
    }
}
