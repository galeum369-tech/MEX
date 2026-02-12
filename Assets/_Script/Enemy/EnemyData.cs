using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Scriptable Object/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;      // 이름
    public GameObject prefab;     // 소환할 프리팹

    [Header("기본 스탯")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float detectionRange = 10f; // 플레이어 인식 거리

    [Header("전투 스탯")]
    public float damage = 10f;
    public float attackRange = 2f;    // 공격 사거리
    public float attackCooldown = 1.5f; // 공격 주기

    [Header("타입 분류")]
    public bool isMelee = true;       // true면 근거리, false면 원거리
    public bool isElite = false;      // 엘리트 여부
}