using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Data")]
    public PlayerData playerData;
    public InventoryData playerInventory; // 참조용

    [Header("Map Transition")]
    public Vector2 targetSpawnPos;
    public Vector2 lastWorldPos;
    public bool isTransitioning = false;
    public bool useRandomSpawn = false;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (playerData == null) InitGame();
    }

    void InitGame()
    {
        playerData = new PlayerData();
        playerInventory = playerData.inventoryData;
        Debug.Log("🆕 게임 데이터 초기화 완료");
    }

    // [통합] 스폰 포인트 매니저 삭제 후 여기로 이동
    public Vector2 GetRandomSpawnPosition()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (points.Length == 0) return Vector2.zero;
        return points[Random.Range(0, points.Length)].transform.position;
    }
}