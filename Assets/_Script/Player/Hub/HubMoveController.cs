using UnityEngine;

public class HubMoveController
{
    Rigidbody2D rb;

    public HubMoveController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    public void Move(float xDirection, float speed)
    {
        // [수정] X축 속도를 직접 지정 (미끄러짐 방지)
        // Y축(중력)은 건드리지 않음
        rb.linearVelocity = new Vector2(xDirection * speed, rb.linearVelocity.y);
    }

    public void Jump(float jumpForce)
    {
        // [수정] 점프 전 Y속도 0으로 초기화 (일정한 높이)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void FastFall(float fallSpeed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fallSpeed);
    }

    public void Stop()
    {
        // [수정] 즉시 정지
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}