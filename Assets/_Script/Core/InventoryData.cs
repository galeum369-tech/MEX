using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public InventoryContainer mechInventory = new InventoryContainer();
    public InventoryContainer shipInventory = new InventoryContainer();

    // [★추가] 편의 함수
    public void AddItemToMech(int itemID, int count)
    {
        var existingItem = mechInventory.cargoSlots.Find(x => x.itemID == itemID);

        if (existingItem != null)
        {
            existingItem.count += count;
            Debug.Log($"📦 아이템({itemID}) 추가됨. 현재 개수: {existingItem.count}");
        }
        else
        {
            mechInventory.cargoSlots.Add(new InventoryItem { itemID = itemID, count = count });
            Debug.Log($"📦 새 아이템({itemID}) 획득!");
        }
    }
}