using UnityEngine;

public class ShipMoveController
{
    private Rigidbody2D rb;

    public ShipMoveController(Rigidbody2D rigidbody)
    {
        this.rb = rigidbody;
    }

    // [핵심] 가속도(acceleration)를 받아서 부드럽게 속도를 올림
    public void Move(Vector2 direction, float speed, float acceleration)
    {
        Vector2 targetVelocity = direction * speed;

        // 현재 속도 -> 목표 속도로 'acceleration'만큼 서서히 변화 (Lerp)
        // acceleration이 높으면 칼반응, 낮으면 미끄러짐
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }

    // 회전 (기존과 동일)
    public void Rotate(Vector2 direction, float turnSpeed)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(angle);
        }
    }
}