using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

public class InventoryUI : MonoBehaviour
{
    [Header("프리팹 & 위치")]
    public GameObject slotPrefab;     // 아까 만든 슬롯 프리팹 (Icon, Count 있는 거)

    [Header("슬롯이 생성될 위치 (Content)")]
    public Transform cargoContent;    // 일반 화물칸 (Grid Layout Group 있는 놈)
    public Transform safeContent;     // (선택) 보호 슬롯칸 (없으면 비워도 됨)

    // 매니저가 이 함수를 호출해서 화면을 갱신함
    public void RefreshUI(InventoryContainer _container)
    {
        // 1. 기존 슬롯 싹 지우기 (청소)
        ClearSlots(cargoContent);
        if (safeContent != null) ClearSlots(safeContent);

        // 2. 화물칸(Cargo) 그리기
        DrawSlots(_container.cargoSlots, cargoContent);

        // 3. 보호칸(Safe) 그리기 (있다면)
        if (safeContent != null)
        {
            DrawSlots(_container.safeSlots, safeContent);
        }
    }

    // 내부적으로 쓰는 그리기 함수
    void DrawSlots(System.Collections.Generic.List<InventoryItem> _list, Transform _parent)
    {
        foreach (var item in _list)
        {
            // 슬롯 생성
            GameObject newSlot = Instantiate(slotPrefab, _parent);

            // 데이터 연동 (아이콘, 개수)
            Image iconImg = newSlot.transform.Find("Icon_Image").GetComponent<Image>();
            TextMeshProUGUI countText = newSlot.transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

            if (item.Data != null)
            {
                iconImg.sprite = item.Data.itemIcon;
                iconImg.enabled = true;
            }

            // 개수가 1보다 클 때만 숫자 표시 (1개면 깔끔하게 숫자 숨김)
            if (item.count > 1)
                countText.text = item.count.ToString();
            else
                countText.text = "";
        }
    }

    // 청소 함수
    void ClearSlots(Transform _parent)
    {
        foreach (Transform child in _parent)
        {
            Destroy(child.gameObject);
        }
    }
}