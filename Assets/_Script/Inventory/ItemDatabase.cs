using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;
    private Dictionary<int, ItemData> itemDict = new Dictionary<int, ItemData>();

    private void Awake()
    {
        instance = this;
        ItemData[] items = Resources.LoadAll<ItemData>("Items"); // Resources/Items 폴더 필수!
        foreach (var item in items)
        {
            if (!itemDict.ContainsKey(item.itemID)) itemDict.Add(item.itemID, item);
        }
    }

    public ItemData GetItem(int _id)
    {
        if (itemDict.ContainsKey(_id)) return itemDict[_id];
        return null;
    }
}