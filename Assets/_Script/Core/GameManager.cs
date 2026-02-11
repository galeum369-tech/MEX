using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Data")]
    public PlayerData playerData;

    // [★추가됨] 인벤토리 데이터 변수 선언 (이제 빨간줄 사라짐)
    public InventoryData playerInventory;

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
        // playerData나 playerInventory 둘 중 하나라도 없으면 초기화
        if (playerData == null || playerInventory == null)
        {
            InitGame();
        }
    }

    void InitGame()
    {
        // 1. 플레이어 스탯 등 기본 데이터
        playerData = new PlayerData();

        // 2. [★추가됨] 인벤토리 데이터 생성 (메카/수송선 가방 만들기)
        playerInventory = new InventoryData();

        // (선택) 테스트용 기본 아이템 지급이 필요하면 여기서
        // playerInventory.AddToMech(1001, 5); // 예: 고철 5개 지급

        Debug.Log("🆕 새 게임 데이터 & 인벤토리 초기화 완료");
    }
}