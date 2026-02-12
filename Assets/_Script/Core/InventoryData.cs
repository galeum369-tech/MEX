using UnityEngine;
using System.Collections.Generic;

// [1] 아이템 슬롯 (기존 유지)
[System.Serializable]
public class InventoryItem
{
    public int itemID;
    public int count;
    public ItemData Data { get { return ItemDatabase.instance.GetItem(itemID); } }
}

// [퀵슬롯 데이터 구조 추가]
[System.Serializable]
public class QuickSlotData
{
    // 각 슬롯(1~4번)에 등록된 아이템의 ID를 저장 (0이면 비어있음)
    public int[] slotItemIDs = new int[4];
}

// [2] 가방 (기존 유지)
[System.Serializable]
public class InventoryContainer
{
    [Header("위험 구역 (사망 시 증발)")]
    public List<InventoryItem> cargoSlots = new List<InventoryItem>();

    [Header("안전 구역 (사망 시 유지)")]
    public List<InventoryItem> safeSlots = new List<InventoryItem>();
    public int maxSafeSlotCount = 3;

    public void OnMissionFail()
    {
        cargoSlots.Clear();
        Debug.Log("💀 임무 실패! 화물칸의 아이템을 모두 잃었습니다...");
    }

    public void AddItem(int _itemID, int _count)
    {
        AddToSpecificList(cargoSlots, _itemID, _count, 999);
    }

    private void AddToSpecificList(List<InventoryItem> _list, int _id, int _count, int _maxSlots)
    {
        ItemData data = ItemDatabase.instance.GetItem(_id);
        if (data == null) return;

        var existing = _list.Find(x => x.itemID == _id && x.count < data.maxStack);

        if (existing != null)
        {
            int space = data.maxStack - existing.count;
            if (_count <= space) existing.count += _count;
            else
            {
                existing.count = data.maxStack;
                if (_list.Count < _maxSlots)
                    _list.Add(new InventoryItem { itemID = _id, count = _count - space });
            }
        }
        else
        {
            if (_list.Count < _maxSlots)
                _list.Add(new InventoryItem { itemID = _id, count = _count });
        }
    }
}

// [3] 플레이어 전체 인벤토리 데이터 (수정됨)
[System.Serializable]
public class InventoryData
{
    [Header("재화")]
    public int money = 0; // 허브에서 표시될 재화

    [Header("기체별 가방")]
    public InventoryContainer mechInventory = new InventoryContainer();
    public InventoryContainer shipInventory = new InventoryContainer();

    [Header("기체별 독립 퀵슬롯")]
    // 수송선과 메카가 각각 4개씩 독립적인 퀵슬롯을 가짐
    public QuickSlotData mechQuickSlots = new QuickSlotData();
    public QuickSlotData shipQuickSlots = new QuickSlotData();

    public void AddToMech(int id, int count) => mechInventory.AddItem(id, count);

    // 재화 관리 함수
    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"💰 재화 획득! 현재 자산: {money}");
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        Debug.Log("❌ 잔액이 부족합니다.");
        return false;
    }

    public void HandleDeath()
    {
        mechInventory.OnMissionFail();
        // 필요 시 사망할 때 퀵슬롯의 소모성 아이템 처리 로직도 여기에 추가 가능
    }
}