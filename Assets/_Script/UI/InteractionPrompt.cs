using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Interaction UI")]
    public GameObject promptRoot;      // 프롬프트 부모 오브젝트
    public TextMeshProUGUI promptText; // 상호작용 내용 텍스트

    // 프롬프트 띄우기
    public void ShowPrompt(string message)
    {
        if (promptRoot == null) return;
        promptText.text = message; // 예: "상점 열기"
        promptRoot.SetActive(true);
    }

    // 프롬프트 숨기기
    public void HidePrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }
}