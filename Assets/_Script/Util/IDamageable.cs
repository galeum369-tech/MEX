using UnityEngine;

// 공격받을 수 있는 모든 오브젝트가 상속받을 인터페이스
public interface IDamageable
{
    // 데미지를 입는 함수 (데미지 양, 타격 지점 등 확장 가능)
    void TakeDamage(float damage, float knockback);
}