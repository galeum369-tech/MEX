using UnityEngine;

public class EnemyMoveController
{
    private Rigidbody2D rb;
    private Transform transform;

    public EnemyMoveController(Rigidbody2D rb, Transform transform)
    {
        this.rb = rb;
        this.transform = transform;
    }

    public void Move(float direction, float speed)
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        // 방향에 따른 스프라이트 반전
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // 단차 감지 로직 (레이캐스트)
    public bool CheckStep(Transform lower, Transform upper, float dir, float dist, LayerMask layer)
    {
        bool isLowerBlocked = Physics2D.Raycast(lower.position, Vector2.right * dir, dist, layer);
        bool isUpperBlocked = Physics2D.Raycast(upper.position, Vector2.right * dir, dist, layer);

        // 발밑은 막혔는데 머리 위는 뚫려있으면 "점프 가능한 단차"
        return isLowerBlocked && !isUpperBlocked;
    }
}