using UnityEngine;
using System.Collections;

public class TestSandbag : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    public float maxHp = 100f;
    public float currentHp;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHp = maxHp;

        // 2D니까 SpriteRenderer를 가져옵니다
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // 인터페이스 구현부
    public void TakeDamage(float damage, float knockback)
    {
        currentHp -= damage;
        Debug.Log($"[샌드백] 윽! {damage} 데미지! (남은 HP: {currentHp}) / 넉백: {knockback}");

        // 맞았을 때 빨간색으로 깜빡이기
        if (spriteRenderer != null)
        {
            StopAllCoroutines(); // 기존 깜빡임 취소 (연타 맞을 때 꼬임 방지)
            StartCoroutine(BlinkRoutine());
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator BlinkRoutine()
    {
        spriteRenderer.color = Color.red; // 빨간맛
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        spriteRenderer.color = originalColor; // 원상복구
    }

    private void Die()
    {
        Debug.Log("[샌드백] 파괴되었습니다.");
        currentHp = maxHp; // 테스트를 위해 죽지 않고 부활시킴
    }
}