using UnityEngine;

public class AttackSystem13
{
    Transform attPoint;
    float attRadius;
    int attDamage;
    LayerMask targetLayer;

    public AttackSystem13(Transform attPoint, float attRadius, int attDamage, LayerMask targetLayer)
    {
        this.attPoint = attPoint;
        this.attRadius = attRadius;
        this.attDamage = attDamage;
        this.targetLayer = targetLayer;
    }

    // 애니메이션 이벤트에서 호출
    public void AnimEvent_CheckCollier()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attPoint.position, attRadius, targetLayer);

        foreach (Collider2D hit in hits )
        {
            hit.TryGetComponent<IDamageable13>(out IDamageable13 damageable);
            damageable.TakeDamage(attDamage);
        }
    }

    public void DrawAttackRange()
    {
        if(attPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attPoint.position, attRadius);
        }
    }
}
