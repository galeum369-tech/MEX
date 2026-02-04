using UnityEngine;

public class MechAnimController
{
    Animator anim;

    readonly int hashMove = Animator.StringToHash("Move");
    readonly int hashDash = Animator.StringToHash("Dash");
    readonly int hashCrouch = Animator.StringToHash("Crouch");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashDodge = Animator.StringToHash("Dodge");
    readonly int hashJump = Animator.StringToHash("Jump");
    readonly int hashJumping = Animator.StringToHash("Jumping");
    readonly int hashFall = Animator.StringToHash("Fall");
    readonly int hashLand = Animator.StringToHash("Land");
    readonly int hashDead = Animator.StringToHash("Dead");


    public MechAnimController(Animator animator)
    {
        anim = animator;
    }

    public void PlayMove(bool isMoving)
    {
        anim.SetBool(hashMove, isMoving);
    }

    public void PlayDash(bool isDashing)
    {
        anim.SetBool(hashDash, isDashing);
    }

    public void PlayCrouch(bool isCrouching)
    {
        anim.SetBool(hashCrouch, isCrouching);
    }

    public void PlayAttack()
    {
        anim.SetTrigger(hashAttack);
    }

    public void PlayDodge()
    {
        anim.SetTrigger(hashDodge);
    }

    public void PlayJump()
    {
        anim.SetTrigger(hashJump);
    }

    public void PlayJumping(bool isJumping)
    {
        anim.SetBool(hashJumping, isJumping);
    }

    public void PlayFall(bool isFalling)
    {
        anim.SetBool(hashFall, isFalling);
    }

    public void PlayLand()
    {
        anim.SetTrigger(hashLand);
    }

    public void PlayDead(bool isDead)
    {
        anim.SetBool(hashDead, isDead);
    }
}