using UnityEngine;
using System.Collections.Generic;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance;

    [Header("Safe Spawn Points (Drag Transforms here)")]
    public List<Transform> spawnPoints = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public Vector2 GetRandomSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("⚠️ 스폰 포인트가 없습니다! (0,0) 반환");
            return Vector2.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex].position;
    }
}