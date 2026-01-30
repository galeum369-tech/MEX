using UnityEngine;

// 우클릭 -> Create -> Data -> Weapon -> Melee 로 생성 가능
[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Data/Weapon/Melee")]
public class MeleeWeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public int weaponID;        // 무기 고유 ID
    public string weaponName;   // 이름 (예: 파일 벙커)
    [TextArea] public string description; // 설명
    public Sprite icon;         // UI 아이콘

    [Header("전투 스탯")]
    public float damage = 20f;       // 데미지
    public float cooldown = 0.5f;    // 공격 후딜 (경직 시간)
    public float knockbackForce = 5f;// 적을 밀어내는 힘
    public float hitStopDuration = 0.1f; // 타격 시 역경직 (타격감)

    [Header("옵션")]
    public float moveSpeedModifier = 0f; // 무거우면 이속 감소 (-1 등)
}