using UnityEngine;

// 프로젝트 창 우클릭 -> Create -> Item Data로 생성 가능하게 함
[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Object/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public int itemID;              // 고유 ID (예: 1001)
    public string itemName;         // 아이템 이름 (예: "고철 덩어리")
    public Sprite itemIcon;         // 아이콘 이미지
    [TextArea]
    public string description;      // 설명 (예: "평범한 고철이다. 상점에 팔 수 있다.")

    [Header("타입 설정")]
    public ItemType itemType;       // 재료? 소모품? 키아이템?

    [Header("속성")]
    public int maxStack = 99;       // 한 칸에 몇 개까지 겹쳐지는가?
    public int price = 0;           // 상점 판매가/구입가

    [Header("소모품 전용 (재료면 무시)")]
    public float effectValue;       // 예: 체력 회복량, 배터리 충전량 등
}

// 장비를 제외한 깔끔한 3단 분류
public enum ItemType
{
    Resource,   // 재료 (고철, 광물, 전리품 - 주로 판매/제작용)
    Consumable, // 소모품 (수리 키트, 에너지 팩, 버프 물약)
    KeyItem     // 중요 (퀘스트 아이템, 보안 카드 - 버리기 불가)
}