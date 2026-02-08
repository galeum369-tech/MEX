using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerData playerData;

    [Header("Map Transition Info")]
    public Vector2 targetSpawnPos;       // 이동할 목표 위치
    public Vector2 lastWorldPos;         // 되돌아올 때를 위한 저장 위치
    public bool isTransitioning = false; // 현재 이동 중인가?
    public bool useRandomSpawn = false;  // 랜덤 스폰을 쓸 것인가?

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 데이터가 없으면 초기화 (씬 이동 시 유지)
        if (playerData == null)
        {
            InitGame();
        }
    }

    void InitGame()
    {
        playerData = new PlayerData();
        Debug.Log("🆕 새 게임 데이터 초기화 완료");
    }
}