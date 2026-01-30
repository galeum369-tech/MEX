using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class WeaponHitbox : MonoBehaviour
{
    // 공격 한 번에 같은 놈 두 번 때리는 거 방지
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    private float currentDamage;
    private float currentKnockback;

    // 공격 시작할 때 플레이어가 호출해주는 함수 (데이터 주입)
    public void Initialize(float damage, float knockback)
    {
        this.currentDamage = damage;
        this.currentKnockback = knockback;

        // 때린 놈 목록 초기화 (새로운 공격이니까)
        hitTargets.Clear();
    }

    // 콜라이더가 켜지고 적과 닿았을 때 실행됨
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 적(Enemy) 태그 확인
        if (collision.CompareTag("Enemy"))
        {
            GameObject target = collision.gameObject;

            // 2. 이미 맞은 놈이면 패스
            if (hitTargets.Contains(target)) return;

            // 3. 타격 처리
            hitTargets.Add(target); // 목록에 등록

            Debug.Log($"💥 [Hit] {target.name}에게 {currentDamage} 데미지!");

            // [나중에 구현] 적에게 데미지 전달
            // var enemy = target.GetComponent<IDamageable>();
            // if (enemy != null) enemy.TakeDamage(currentDamage, currentKnockback);
        }
        // (옵션) 파괴 가능한 오브젝트(상자 등)도 여기서 처리 가능
    }
}
