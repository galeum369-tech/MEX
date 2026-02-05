using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class WeaponHitbox : MonoBehaviour
{
    [Header("타격 설정")]
    // 인스펙터에서 때릴 레이어를 선택할 수 있습니다 (예: Enemy, Destructible 등 다중 선택 가능)
    public LayerMask targetLayers;

    // 공격 한 번에 같은 놈 두 번 때리는 거 방지
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    private float currentDamage;
    private float currentKnockback;

    public void Initialize(float damage, float knockback)
    {
        this.currentDamage = damage;
        this.currentKnockback = knockback;
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // [변경점] 태그 비교 대신 레이어 비트 연산으로 확인
        // "부딪힌 놈의 레이어가 targetLayers에 포함되어 있는가?" 확인하는 로직입니다.
        if ((targetLayers.value & (1 << collision.gameObject.layer)) > 0)
        {
            GameObject target = collision.gameObject;

            // 이미 맞은 놈이면 패스
            if (hitTargets.Contains(target)) return;

            // 타격 처리
            hitTargets.Add(target);
            Debug.Log($"💥 [Hit] {target.name} (Layer: {LayerMask.LayerToName(target.layer)}) 타격 성공!");

            // 데미지 전달
            IDamageable enemy = target.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage, currentKnockback);
            }
        }
    }
}