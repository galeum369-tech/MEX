using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Detection")]
    public LayerMask targetLayer;    // Player 레이어

    [Header("Destination")]
    public string sceneToLoad;       // 이동할 씬 이름

    [Header("Spawn Logic")]
    public Vector2 fixedSpawnPos;    // 고정 이동 좌표
    public bool isReturnPortal;      // 저장된 위치로 복귀?
    public bool saveCurrentPos;      // 현재 위치를 저장?
    public bool isRandomSpawn;       // 랜덤 위치 사용?

    [Header("UI Events")]
    public UnityEvent onPlayerEnter; // UI 켜기
    public UnityEvent onPlayerExit;  // UI 끄기

    private PlayerInputHandler currentInputHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 레이어 체크
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            currentInputHandler = collision.GetComponent<PlayerInputHandler>();

            if (currentInputHandler != null)
            {
                // F키 이벤트 연결
                currentInputHandler.OnInteract += ExecutePortalMove;

                // UI 켜기
                onPlayerEnter?.Invoke();
                Debug.Log("포탈 진입: F키를 눌러 이동");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            if (currentInputHandler != null)
            {
                // 연결 해제
                currentInputHandler.OnInteract -= ExecutePortalMove;
                currentInputHandler = null;

                // UI 끄기
                onPlayerExit?.Invoke();
            }
        }
    }

    // F키를 누르면 실행되는 함수
    private void ExecutePortalMove()
    {
        // 중복 실행 방지
        if (currentInputHandler != null)
            currentInputHandler.OnInteract -= ExecutePortalMove;

        onPlayerExit?.Invoke(); // UI 끄고 이동

        // 1. 위치 저장
        if (saveCurrentPos && GameManager.instance != null)
        {
            GameManager.instance.lastWorldPos = this.transform.position;
            Debug.Log($"📌 위치 저장됨: {GameManager.instance.lastWorldPos}");
        }

        // 2. 이동 설정
        if (GameManager.instance != null)
        {
            if (isReturnPortal)
            {
                GameManager.instance.useRandomSpawn = false;
                GameManager.instance.targetSpawnPos = GameManager.instance.lastWorldPos;
            }
            else if (isRandomSpawn)
            {
                GameManager.instance.useRandomSpawn = true;
            }
            else
            {
                GameManager.instance.useRandomSpawn = false;
                GameManager.instance.targetSpawnPos = fixedSpawnPos;
            }

            GameManager.instance.isTransitioning = true;
        }

        // 3. 씬 로드
        if (SceneController.Instance != null)
            SceneController.Instance.LoadScene(sceneToLoad);
        else
            SceneManager.LoadScene(sceneToLoad);
    }
}