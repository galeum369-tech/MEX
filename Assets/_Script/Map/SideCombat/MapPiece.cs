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
    [SerializeField] private GameObject enemyPrefab;

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
        if (enemyPrefab == null) return;

        // 적 생성
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // 생성된 적의 부모를 이 맵 조각으로 설정 (Hierarchy 정리용)
        enemyObj.transform.SetParent(this.transform);

        // [참고] 나중에 적의 스탯(체력 등)을 난이도에 맞게 보정하는 로직을 여기에 추가 가능
        // EnemyController enemyScript = enemyObj.GetComponent<EnemyController>();
        // if (enemyScript != null) enemyScript.ApplyDifficulty(difficulty);
    }
}