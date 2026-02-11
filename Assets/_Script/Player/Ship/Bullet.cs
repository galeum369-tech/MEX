using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float knockback = 5f; // 넉백 파워 추가
    public float lifeTime = 3f;

    private void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed; // 2D는 right 기준
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other) // 2D 충돌 감지
    {
        // 부딪힌 대상에게 "너 IDamageable 명찰 있어?" 하고 물어봄
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
        {
            // 명찰이 있으면 때린다!
            target.TakeDamage(damage, knockback);

            // 총알은 할 일을 다했으니 사라짐
            Destroy(gameObject);
        }
        else
        {
            // (선택) 벽에 닿으면 사라지기 등등
            // Destroy(gameObject);
        }
    }
}