using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Prefabs")]
    [SerializeField] private MapPiece startMapPrefab;   // 시작 맵
    [SerializeField] private MapPiece endMapPrefab;     // 보스/종료 맵
    [SerializeField] private List<MapPiece> battleMapPrefabs; // 중간 전투 맵들 (랜덤)

    [Header("Settings")]
    [SerializeField] private int totalBattleMaps = 3;   // 중간에 몇 개를 끼워 넣을지

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        // 1. 현재 연결 지점 (초기값은 (0,0,0))
        Vector3 currentPosition = Vector3.zero;

        // 2. [시작 맵] 생성
        // 시작 맵은 무조건 (0,0,0)에 생성
        MapPiece startMap = Instantiate(startMapPrefab, currentPosition, Quaternion.identity);

        // 다음 맵이 붙을 위치 갱신
        currentPosition = startMap.nextPoint.position;

        // 3. [전투 맵] 반복 생성
        for (int i = 0; i < totalBattleMaps; i++)
        {
            // 랜덤으로 맵 하나 뽑기
            int randomIndex = Random.Range(0, battleMapPrefabs.Count);
            MapPiece selectedPrefab = battleMapPrefabs[randomIndex];

            // 맵 생성 (현재 연결 지점에)
            MapPiece newMap = Instantiate(selectedPrefab, currentPosition, Quaternion.identity);

            // 중요: 다음 맵을 위해 연결 지점 갱신
            currentPosition = newMap.nextPoint.position;
        }

        // 4. [종료 맵] 생성
        Instantiate(endMapPrefab, currentPosition, Quaternion.identity);
    }
}