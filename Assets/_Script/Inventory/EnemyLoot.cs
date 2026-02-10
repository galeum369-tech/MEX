using UnityEngine;
using System.Collections.Generic;

public class EnemyLoot : MonoBehaviour
{
    [System.Serializable]
    public class DropTable
    {
        public ItemData item;
        [Range(0, 100)] public float dropChance;
        public int minCount = 1;
        public int maxCount = 1;
    }

    public GameObject wreckagePrefab; // 죽으면 생길 '잔해(LootableObject)' 프리팹
    public List<DropTable> drops;

    public void SpawnLoot()
    {
        if (wreckagePrefab == null) return;

        // 잔해 생성
        GameObject wreckage = Instantiate(wreckagePrefab, transform.position, Quaternion.identity);
        LootableObject lootObj = wreckage.GetComponent<LootableObject>();

        // 잔해 인벤토리에 아이템 채워넣기
        if (lootObj != null)
        {
            foreach (var drop in drops)
            {
                if (Random.Range(0f, 100f) <= drop.dropChance)
                {
                    int count = Random.Range(drop.minCount, drop.maxCount + 1);
                    lootObj.lootInventory.AddItem(drop.item.itemID, count);
                }
            }
        }
    }
}