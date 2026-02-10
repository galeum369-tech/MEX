using UnityEngine;

public class LootableObject : MonoBehaviour
{
    // 적 시체도 'InventoryContainer'를 가짐 (플레이어랑 똑같은 구조!)
    public InventoryContainer lootInventory = new InventoryContainer();

    // 상호작용(F키) 시 호출
    public void Interact()
    {
        Debug.Log("📦 루팅 시작!");
        // 나중에 UI 매니저에게 lootInventory를 넘겨주면 됨
    }
}