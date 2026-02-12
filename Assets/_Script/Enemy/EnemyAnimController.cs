using UnityEngine;

public class EnemyAnimController
{
    private Animator anim;

    public EnemyAnimController(Animator anim)
    {
        this.anim = anim;
    }

    public void PlayMove(bool isMoving) => anim.SetBool("IsMoving", isMoving);
    public void PlayAttack() => anim.SetTrigger("Attack");
    public void PlayHurt() => anim.SetTrigger("Hurt");
    public void PlayDie() => anim.SetTrigger("Die");

    // 원거리 몹 전용이나 엘리트 전용 파라미터가 있다면 추가
    public void PlaySpecial() => anim.SetTrigger("Special");
}