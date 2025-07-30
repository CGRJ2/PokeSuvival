using NTJ;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items;

    private Dictionary<int, ItemData> itemDict;

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
            return _instance;
        }
    }

    private void OnEnable()
    {
        itemDict = items.ToDictionary(i => i.id);
    }

    public ItemData GetItemById(int id)
    {
        return itemDict != null && itemDict.TryGetValue(id, out var data) ? data : null;
    }
}
