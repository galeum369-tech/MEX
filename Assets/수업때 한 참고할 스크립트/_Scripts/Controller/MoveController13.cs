using UnityEngine;

public class MoveController13
{
    Rigidbody2D rigidbody;

    // 생성자 (DI)
    public MoveController13(Rigidbody2D rigidbody)
    {
        this.rigidbody = rigidbody;
    }

    public void Move(Vector2 direction, float speed)
    {
        rigidbody.linearVelocity = direction * speed;
    }
    public void Stop()
    {
        rigidbody.linearVelocity = Vector2.zero;
    }
}
