using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public string interactionMessage = "상호작용"; // [F] 뒤에 붙을 글자
    public UnityEvent onInteract;                // F 눌렀을 때 실행될 일들

    private bool isPlayerInRange = false;
    private PlayerInputHandler currentInput;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentInput = other.GetComponent<PlayerInputHandler>();

            // 이벤트 연결
            if (currentInput != null) currentInput.OnInteract += ExecuteInteract;

            // HUD에 프롬프트 표시
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowPrompt(interactionMessage);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (currentInput != null) currentInput.OnInteract -= ExecuteInteract;

            // 프롬프트 숨기기
            if (HUDManager.Instance != null)
                HUDManager.Instance.HidePrompt();
        }
    }

    private void ExecuteInteract()
    {
        if (isPlayerInRange)
        {
            Debug.Log($"{gameObject.name} 상호작용 실행!");
            onInteract?.Invoke(); // 인스펙터에서 설정한 UI 열기 등 실행
        }
    }
}