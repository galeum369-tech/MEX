using UnityEngine;

public class HubMoveController
{
    Rigidbody2D rb;

    public HubMoveController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    //좌우 이동
    public void Move(float xDirection, float speed)
    {
        rb.linearVelocity = new Vector2(xDirection * speed, rb.linearVelocity.y);
    }

    //점프
    public void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    //빠른 하강
    public void FastFall(float fallSpeed)
    {
        // 현재 이미 떨어지고 있는 속도보다 더 빠르게 내리꽂고 싶을 때만 적용
        // (실수로 점프 중에 눌러서 점프가 씹히는 걸 방지하려면 y < 0 조건을 걸 수도 있음)
        // 하지만 '즉시 착지' 느낌을 원하면 조건 없이 덮어씌우는 게 나음.
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fallSpeed);
    }

    //정지
    public void Stop()
    {
        //옆으로의 움직임만 없앰
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

}
