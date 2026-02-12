using UnityEngine;
using System.Collections.Generic;

public class MapPiece : MonoBehaviour
{
    [Header("Connections")]
    // 맵의 오른쪽 끝, 다음 맵이 붙을 연결 지점
    public Transform nextPoint;

    [Header("Combat Settings")]
    // 에디터에서 맵 프리팹 내의 스폰 포인트들을 드래그해서 넣어주세요.
    [SerializeField] private List<Transform> enemySpawnPoints;

    // 소환할 적 프리팹 (나중에 EnemyManager 등으로 분리 가능)
    [SerializeField] private List<EnemyData> spawnableEnemyData;

    /// <summary>
    /// 맵이 생성될 때 MapGenerator에 의해 호출되는 초기화 함수
    /// </summary>
    /// <param name="difficulty">필드에서 전달받은 난이도 (1, 2, 3)</param>
    public void Initialize(int difficulty)
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Count == 0) return;

        // 1. 난이도에 따른 스폰 마리수 결정 로직
        // 난이도가 높을수록 더 많은 적이 나옵니다.
        int minSpawn = difficulty;
        int maxSpawn = difficulty * 2;
        int targetCount = Random.Range(minSpawn, maxSpawn + 1);

        // 2. 가용한 스폰 포인트 리스트 복사
        List<Transform> availablePoints = new List<Transform>(enemySpawnPoints);

        // 3. 적 소환 실행
        for (int i = 0; i < targetCount; i++)
        {
            if (availablePoints.Count == 0) break;

            // 랜덤하게 포인트 선택
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndex];

            // 적 생성
            SpawnEnemy(spawnPoint, difficulty);

            // 한 포인트에서 중복 소환되지 않도록 리스트에서 제거
            availablePoints.RemoveAt(randomIndex);
        }
    }

    private void SpawnEnemy(Transform spawnPoint, int difficulty)
    {
        if (spawnableEnemyData == null || spawnableEnemyData.Count == 0) return;

        // 1. 리스트에서 랜덤하게 적 데이터 하나 뽑기
        EnemyData selectedData = spawnableEnemyData[Random.Range(0, spawnableEnemyData.Count)];

        // 2. 데이터에 연결된 프리팹 생성
        GameObject enemyObj = Instantiate(selectedData.prefab, spawnPoint.position, Quaternion.identity);

        // 3. 부모 설정
        enemyObj.transform.SetParent(this.transform);

        // 4. 초기화 및 난이도 주입
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            // SO 데이터를 적에게 직접 전달 (스탯 설정용)
            enemy.data = selectedData;
            enemy.Init(difficulty);
        }
    }

}