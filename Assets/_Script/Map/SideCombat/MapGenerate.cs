using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Prefabs")]
    [SerializeField] private MapPiece startMapPrefab;
    [SerializeField] private MapPiece endMapPrefab;
    [SerializeField] private List<MapPiece> battleMapPrefabs;

    private void Start()
    {
        // GameManager에서 난이도를 가져옴 (기본값 1)
        int difficulty = 1;
        if (GameManager.instance != null)
        {
            // 필드에서 넘어올 때 설정된 난이도 사용
            // difficulty = GameManager.instance.currentDifficulty; 
        }

        GenerateDungeon(difficulty);
    }

    public void GenerateDungeon(int difficulty)
    {
        Vector3 currentPosition = Vector3.zero;

        // 1. 시작 맵 생성
        MapPiece startMap = Instantiate(startMapPrefab, currentPosition, Quaternion.identity);
        currentPosition = startMap.nextPoint.position;

        // 2. 난이도별 맵 개수 결정
        int totalBattleMaps = (difficulty == 1) ? Random.Range(2, 3) :
                              (difficulty == 2) ? Random.Range(3, 5) : Random.Range(4, 6);

        // 3. 전투 맵 랜덤 생성
        for (int i = 0; i < totalBattleMaps; i++)
        {
            MapPiece selectedPrefab = battleMapPrefabs[Random.Range(0, battleMapPrefabs.Count)];
            MapPiece newMap = Instantiate(selectedPrefab, currentPosition, Quaternion.identity);

            // 몬스터 스폰 등 초기화 (난이도 전달)
            newMap.Initialize(difficulty);

            currentPosition = newMap.nextPoint.position;
        }

        // 4. 종료 맵 생성
        Instantiate(endMapPrefab, currentPosition, Quaternion.identity);
    }
}