using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryViewManager;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Panels")]
    public GameObject mechPanel;
    public GameObject shipPanel;

    [Header("Hub Specific UI")]
    public GameObject hubPanel;      // 허브용 패널 (재화 전용)
    public TextMeshProUGUI moneyText; // 돈 표시 텍스트

    // --- [추가] 상호작용 프롬프트 UI 영역 ---
    [Header("Interaction UI")]
    public GameObject promptRoot;      // 프롬프트 부모 오브젝트 (평소엔 비활성)
    public TextMeshProUGUI promptText; // "[F] 상호작용" 등의 글자가 표시될 텍스트

    [Header("===== Mech UI =====")]
    public Slider mechHpBar;
    public Slider mechSkillBar;  // 스킬 게이지
    public Slider mechRepairBar; // 회복 게이지

    [Header("===== Ship UI =====")]
    public Slider shipHpBar;
    public Slider shipSkillBar;
    public Slider shipRepairBar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 실시간 재화 갱신 (Hub 혹은 Field에서 보임)
        if (hubPanel != null && hubPanel.activeInHierarchy && GameManager.instance != null)
        {
            UpdateMoneyUI(GameManager.instance.playerInventory.money);
        }
    }

    /// <summary>
    /// 인벤토리나 메뉴가 열릴 때 모든 HUD 요소를 숨깁니다.
    /// </summary>
    public void HideAllHUD()
    {
        if (hubPanel != null) hubPanel.SetActive(false);
        if (mechPanel != null) mechPanel.SetActive(false);
        if (shipPanel != null) shipPanel.SetActive(false);

        // 인벤토리 오픈 시 프롬프트도 같이 숨김 처리
        HidePrompt();
    }

    /// <summary>
    /// 게임 상태에 따라 평상시 보여줄 HUD를 설정합니다.
    /// </summary>
    public void SetHUDState(GameState state)
    {
        HideAllHUD(); // 일단 전체 숨김

        switch (state)
        {
            case GameState.Hub:
                // 허브: 오직 재화 UI만 표시
                if (hubPanel != null) hubPanel.SetActive(true);
                break;

            case GameState.Field:
                // 필드: 재화 UI + 수송선 상태(체력/퀵슬롯 패널) 표시
                if (hubPanel != null) hubPanel.SetActive(true);
                if (shipPanel != null) shipPanel.SetActive(true);
                break;

            case GameState.Combat:
                // 전투: 메카닉 상태 표시
                if (mechPanel != null) mechPanel.SetActive(true);
                break;
        }
    }

    // --- [추가] 상호작용 프롬프트 제어 함수 ---

    /// <summary>
    /// 상호작용 프롬프트를 화면에 띄웁니다.
    /// </summary>
    public void ShowPrompt(string message)
    {
        if (promptRoot == null || promptText == null) return;

        promptText.text = $"[F] {message}"; // 입력받은 메시지 표시
        promptRoot.SetActive(true);
    }

    /// <summary>
    /// 상호작용 프롬프트를 화면에서 숨깁니다.
    /// </summary>
    public void HidePrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    // --- 데이터 업데이트 함수들 ---

    public void UpdateMoneyUI(int amount)
    {
        if (moneyText != null) moneyText.text = amount.ToString("N0"); // 1,000 단위 콤마 추가
    }

    // --- 메카닉 업데이트 ---
    public void UpdateMechHP(float current, float max)
    {
        if (mechHpBar != null) mechHpBar.value = current / max;
    }
    public void UpdateMechResource(float skillCur, float skillMax, float repairCur, float repairMax)
    {
        if (mechSkillBar != null) mechSkillBar.value = skillCur / skillMax;
        if (mechRepairBar != null) mechRepairBar.value = repairCur / repairMax;
    }

    // --- 수송선 업데이트 ---
    public void UpdateShipHP(float current, float max)
    {
        if (shipHpBar != null) shipHpBar.value = current / max;
    }
    public void UpdateShipResource(float skillCur, float skillMax, float repairCur, float repairMax)
    {
        if (shipSkillBar != null) shipSkillBar.value = skillCur / skillMax;
        if (shipRepairBar != null) shipRepairBar.value = repairCur / repairMax;
    }
}