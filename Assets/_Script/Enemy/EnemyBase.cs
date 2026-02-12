using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public EnemyData data;
    protected EnemyMoveController mc;
    protected EnemyAnimController ac;
    protected Rigidbody2D rb;
    protected Animator anim;

    [Header("Detection & Raycast")]
    public Transform lowerRayPoint;
    public Transform upperRayPoint;
    public LayerMask groundLayer;

    protected float currentHealth;
    protected float currentDamage;
    protected float currentMoveSpeed;

    public virtual void Init(int difficulty)
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 컨트롤러 초기화
        mc = new EnemyMoveController(rb, transform);
        ac = new EnemyAnimController(anim);

        // 난이도에 따른 데이터 보정 (multiplier 등 기존 로직 활용)
        float multiplier = 1f + (difficulty - 1) * 0.5f;
        currentHealth = data.maxHealth * multiplier;
        currentDamage = data.damage * multiplier;
        currentMoveSpeed = data.moveSpeed;
    }
}