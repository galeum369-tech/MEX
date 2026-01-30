using UnityEngine;

public class MecMoveController
{
    private Rigidbody2D rb;

    public MecMoveController(Rigidbody2D rigidbody)
    {
        this.rb = rigidbody;
    }

    // 기본 이동
    public void Move(float xInput, float speed)
    {
        // 입력 방향 * 속도로 리니어 벨로시티 갱신
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);
    }

    // 점프
    public void Jump(float jumpPower)
    {
        // 점프 직전 Y 속도 초기화 (일정한 점프 높이 보장)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    // 빠른 하강 (메카닉이 쿵! 찍는 느낌)
    public void FastFall(float speed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -speed);
    }

    // ★ 중요: 공격 시 제자리 정지용
    public void Stop()
    {
        // X축 속도만 0으로 (공중에서 공격하면 뚝 떨어지게 할지, 관성 남길지는 선택)
        // 여기선 미끄러짐 방지를 위해 완전 정지
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}