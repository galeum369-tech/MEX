using System.Collections.Generic;

[System.Serializable]
public class InventoryContainer
{
    // 보호 슬롯
    public List<InventoryItem> safeSlots = new List<InventoryItem>();

    // 출격 중 획득 슬롯
    public List<InventoryItem> cargoSlots = new List<InventoryItem>();
}

