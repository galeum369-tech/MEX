using UnityEngine;
using UnityEngine.UI; // UI 제어용

public class InventoryViewManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤
    public static InventoryViewManager instance;

    // 게임 상태 정의 (허브/필드 vs 전투)
    public enum GameState { Hub, Field, Combat }
    public GameState currentState = GameState.Field;

    [Header("1. 좌측 패널 그룹 (플레이어)")]
    public GameObject leftGroup;      // 좌측 전체 부모 (Vertical Layout Group 필수!)
    public GameObject shipPanel;      // [위] 수송선 패널 (전투 시 꺼짐)
    public GameObject mechPanel;      // [아래] 메카 패널 (항상 켜짐)

    [Header("2. 우측 패널 그룹 (외부)")]
    public GameObject rightGroup;     // 우측 전체 부모
    public GameObject lootPanel;      // 적 시체 루팅 패널
    public GameObject warehousePanel; // 허브 창고 패널

    [Header("3. UI 스크립트 연결 (데이터 갱신용)")]
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
        // 게임 시작 시 UI 초기화 (일단 다 끄기)
        CloseAll();
    }

    // ====================================================
    // 🎮 모드 전환 (핵심 로직)
    // ====================================================

    // I키를 눌렀을 때 호출: 현재 상태에 맞춰서 내 가방을 염
    public void ToggleInventory()
    {
        // 이미 켜져 있으면 끄기
        if (leftGroup.activeSelf)
        {
            CloseAll();
            return;
        }

        // 켜져 있지 않으면 상태에 맞춰 열기
        OpenPlayerInventory();
    }

    public void OpenPlayerInventory()
    {
        leftGroup.SetActive(true);
        rightGroup.SetActive(false); // 외부 창은 일단 닫고 시작

        // ★ 핵심: 상태에 따른 패널 배치 변경
        switch (currentState)
        {
            case GameState.Hub:
            case GameState.Field:
                // [탐험 모드] : 수송선 + 메카 (2단 분리)
                // Layout Group 덕분에 Ship을 켜면 자동으로 반반 나뉨
                shipPanel.SetActive(true);
                mechPanel.SetActive(true);
                break;

            case GameState.Combat:
                // [전투 모드] : 메카 독점 (1단 합체)
                // Ship을 끄면 Mech가 자동으로 좌측 전체 공간 차지함
                shipPanel.SetActive(false);
                mechPanel.SetActive(true);
                break;
        }

        // 데이터 갱신 (화면에 그리기)
        RefreshPlayerPanels();
    }

    // ====================================================
    // 📦 외부 상호작용 (루팅 / 창고)
    // ====================================================

    // 적 시체(F키)를 눌렀을 때 호출
    public void OpenLooting(InventoryContainer _lootContainer)
    {
        // 1. 내 인벤토리도 같이 열어줌 (그래야 옮기니까)
        OpenPlayerInventory();

        // 2. 우측 패널 켜기
        rightGroup.SetActive(true);
        lootPanel.SetActive(true);
        warehousePanel.SetActive(false); // 창고는 끄고

        // 3. 루팅 데이터 연결
        lootUI.RefreshUI(_lootContainer);
    }

    // 허브 창고를 열었을 때 호출
    public void OpenWarehouse(InventoryContainer _warehouseContainer)
    {
        OpenPlayerInventory();

        rightGroup.SetActive(true);
        lootPanel.SetActive(false); // 루팅은 끄고
        warehousePanel.SetActive(true);

        warehouseUI.RefreshUI(_warehouseContainer);
    }

    // ====================================================
    // 🔄 유틸리티
    // ====================================================

    // 플레이어 쪽 UI만 새로고침 (아이템 옮길 때 호출)
    public void RefreshPlayerPanels()
    {
        if (GameManager.instance != null)
        {
            shipUI.RefreshUI(GameManager.instance.playerInventory.shipInventory);
            mechUI.RefreshUI(GameManager.instance.playerInventory.mechInventory);
        }
    }

    // 모든 창 닫기 (ESC 키 등)
    public void CloseAll()
    {
        leftGroup.SetActive(false);
        rightGroup.SetActive(false);
        // 패널 초기화 등 필요하면 추가
    }
}