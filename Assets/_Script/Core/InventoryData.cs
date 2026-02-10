using UnityEngine;
using System.Collections.Generic;

// [1] 아이템 슬롯 (그대로 유지)
[System.Serializable]
public class InventoryItem
{
    public int itemID;
    public int count;
    public ItemData Data { get { return ItemDatabase.instance.GetItem(itemID); } }
}

// [2] 가방 (수정됨: 보호 슬롯 추가 & 사망 로직 추가)
[System.Serializable]
public class InventoryContainer
{
    [Header("위험 구역 (사망 시 증발)")]
    public List<InventoryItem> cargoSlots = new List<InventoryItem>();

    [Header("안전 구역 (사망 시 유지) - 용량 작음")]
    public List<InventoryItem> safeSlots = new List<InventoryItem>();
    public int maxSafeSlotCount = 3; // 보호 슬롯은 보통 개수가 적음

    // 💀 임무 실패 시 호출! (일반 아이템만 날려버림)
    public void OnMissionFail()
    {
        cargoSlots.Clear(); // 화물칸만 싹 비움
        Debug.Log("💀 임무 실패! 화물칸의 아이템을 모두 잃었습니다...");
    }

    // 아이템 넣기 (기본적으로는 화물칸(Cargo)으로 들어감)
    public void AddItem(int _itemID, int _count)
    {
        // 로직은 아까와 동일하지만 대상이 'cargoSlots'
        AddToSpecificList(cargoSlots, _itemID, _count, 999); // 화물칸은 제한 없음(혹은 크게)
    }

    // 보호 슬롯에 넣기 (UI에서 드래그해서 옮길 때 호출)
    public bool MoveToSafeSlot(InventoryItem _item)
    {
        // 보호 슬롯이 꽉 찼는지 확인
        if (safeSlots.Count >= maxSafeSlotCount)
        {
            Debug.Log("❌ 보호 슬롯이 가득 찼습니다!");
            return false;
        }

        // 화물칸에서 빼고 보호 슬롯에 넣는 로직 필요
        // (상세 구현은 UI 만들 때 하는 게 좋음)
        return true;
    }

    // [내부 함수] 리스트에 아이템 더하는 로직 (중복 제거용)
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
                if (_list.Count < _maxSlots) // 슬롯 제한 체크
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

// [3] 플레이어 데이터 (그대로 유지)
[System.Serializable]
public class InventoryData
{
    public InventoryContainer mechInventory = new InventoryContainer(); // 여기에 safeSlots 3개 설정
    public InventoryContainer shipInventory = new InventoryContainer(); // 창고는 safeSlots 안 씀 (0개 설정)

    public void AddToMech(int id, int count) => mechInventory.AddItem(id, count);

    // ★ 게임오버 시 이거 호출하면 됨
    public void HandleDeath()
    {
        mechInventory.OnMissionFail();
    }
}