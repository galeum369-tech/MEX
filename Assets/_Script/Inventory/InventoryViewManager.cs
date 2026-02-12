using UnityEngine;
using UnityEngine.UI;

public class InventoryViewManager : MonoBehaviour
{
    public static InventoryViewManager instance;

    // 게임 상태 정의 (허브, 필드, 전투)
    public enum GameState { Hub, Field, Combat }
    public GameState currentState = GameState.Field;

    [Header("1. 좌측 패널 그룹 (플레이어 가방)")]
    public GameObject leftGroup;      // 좌측 전체 부모
    public GameObject shipPanel;      // [위] 수송선 패널
    public GameObject mechPanel;      // [아래] 메카 패널

    [Header("2. 우측 패널 그룹 (외부 상호작용)")]
    public GameObject rightGroup;
    public GameObject lootPanel;
    public GameObject warehousePanel;

    [Header("3. 인벤토리 전용 퀵슬롯 편집 그룹")]
    // [수정] 인벤토리를 열었을 때만 나타나는 "모든" 퀵슬롯 설정창
    public GameObject shipQuickSlotEditGroup;
    public GameObject mechQuickSlotEditGroup;

    [Header("4. UI 스크립트 연결")]
    public InventoryUI shipUI;
    public InventoryUI mechUI;
    public InventoryUI lootUI;
    public InventoryUI warehouseUI;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        CloseAll();
    }

    public void ToggleInventory()
    {
        if (leftGroup.activeSelf)
        {
            CloseAll();
            return;
        }
        OpenPlayerInventory();
    }

    public void OpenPlayerInventory()
    {
        leftGroup.SetActive(true);
        rightGroup.SetActive(false);

        // [핵심] 인벤토리가 열리면 'HUDManager'의 평상시 UI(HUD) 요소들을 잠시 숨김
        if (HUDManager.Instance != null) HUDManager.Instance.HideAllHUD();

        // ★ 인벤토리 오픈 시 노출 로직
        switch (currentState)
        {
            case GameState.Hub:
            case GameState.Field:
                // 허브와 필드에서는 두 기체 패널과 편집용 퀵슬롯을 모두 보여줌
                shipPanel.SetActive(true);
                mechPanel.SetActive(true);
                if (shipQuickSlotEditGroup != null) shipQuickSlotEditGroup.SetActive(true);
                if (mechQuickSlotEditGroup != null) mechQuickSlotEditGroup.SetActive(true);
                break;

            case GameState.Combat:
                // 전투 중에는 메카 가방과 메카 퀵슬롯 편집창만 노출
                shipPanel.SetActive(false);
                mechPanel.SetActive(true);
                if (shipQuickSlotEditGroup != null) shipQuickSlotEditGroup.SetActive(false);
                if (mechQuickSlotEditGroup != null) mechQuickSlotEditGroup.SetActive(true);
                break;
        }

        RefreshPlayerPanels();
    }

    // --- 외부 상호작용 로직 ---
    public void OpenLooting(InventoryContainer _lootContainer)
    {
        OpenPlayerInventory();
        rightGroup.SetActive(true);
        lootPanel.SetActive(true);
        warehousePanel.SetActive(false);
        lootUI.RefreshUI(_lootContainer);
    }

    public void OpenWarehouse(InventoryContainer _warehouseContainer)
    {
        OpenPlayerInventory();
        rightGroup.SetActive(true);
        lootPanel.SetActive(false);
        warehousePanel.SetActive(true);
        warehouseUI.RefreshUI(_warehouseContainer);
    }

    public void RefreshPlayerPanels()
    {
        if (GameManager.instance != null)
        {
            shipUI.RefreshUI(GameManager.instance.playerInventory.shipInventory);
            mechUI.RefreshUI(GameManager.instance.playerInventory.mechInventory);
        }
    }

    public void CloseAll()
    {
        leftGroup.SetActive(false);
        rightGroup.SetActive(false);

        // 인벤토리를 닫으면 편집용 퀵슬롯 창도 끔
        if (shipQuickSlotEditGroup != null) shipQuickSlotEditGroup.SetActive(false);
        if (mechQuickSlotEditGroup != null) mechQuickSlotEditGroup.SetActive(false);

        // [핵심] 인벤토리를 닫으면 HUDManager에게 현재 상태에 맞는 '평상시 HUD'를 다시 켜라고 명령
        if (HUDManager.Instance != null) HUDManager.Instance.SetHUDState(currentState);
    }
}