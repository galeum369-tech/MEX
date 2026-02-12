using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // [중요] 인스펙터에서 PlayerInputHandler를 꼭 연결해주세요!
    [SerializeField] private PlayerInputHandler inputHandler;

    public bool IsUIOpen { get; private set; }
    private GameObject currentWindow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- [추가됨] 입력 이벤트 연결 (UIInputController 기능 흡수) ---
    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMove += HandleNavigation;
            inputHandler.OnAttack += ExecuteSelectedUI; // 공격키로 선택
            inputHandler.OnInventory += CloseCurrentWindow; // 인벤키로 닫기
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMove -= HandleNavigation;
            inputHandler.OnAttack -= ExecuteSelectedUI;
            inputHandler.OnInventory -= CloseCurrentWindow;
        }
    }
    // -----------------------------------------------------------

    public void OpenWindow(GameObject window, GameObject firstSelect)
    {
        if (window == null) return;

        window.SetActive(true);
        currentWindow = window;
        IsUIOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (firstSelect != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelect);
        }
    }

    public void CloseCurrentWindow()
    {
        if (currentWindow != null)
        {
            currentWindow.SetActive(false);
            currentWindow = null;
        }

        IsUIOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventSystem.current.SetSelectedGameObject(null);
    }

    // --- [추가됨] UI 조작 함수 (HubPlayer가 호출하는 놈이 얘입니다!) ---
    public void ExecuteSelectedUI()
    {
        if (!IsUIOpen) return;

        // 현재 포커스된 버튼을 찾아서 강제로 누름
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy)
        {
            Button btn = selected.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.Invoke();
                Debug.Log($"[UIManager] {selected.name} 클릭됨");
            }
        }
    }

    private void HandleNavigation(Vector2 input)
    {
        if (!IsUIOpen) return;
        // 메뉴 이동 소리 재생 등 추가 가능
    }
}