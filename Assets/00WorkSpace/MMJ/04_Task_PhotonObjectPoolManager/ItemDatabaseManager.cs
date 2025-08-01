using UnityEngine;
using NTJ;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabaseManager : MonoBehaviour
{
    public static ItemDatabaseManager Instance { get; private set; }

    private Dictionary<int, ItemData> itemDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
        // 
    }

    private void LoadDatabase()
    {
        var db = Resources.Load<ItemDatabase>("ItemDatabase");
        if (db == null || db.items == null)
        {
            Debug.LogError("[ItemDatabaseManager] ItemDatabase 로딩 실패");
            return;
        }

        itemDict = db.items.ToDictionary(i => i.id);
        Debug.Log($"[ItemDatabaseManager] {itemDict.Count}개의 아이템 로딩 완료");
    }

    public ItemData GetItemById(int id)
    {
        return itemDict != null && itemDict.TryGetValue(id, out var item) ? item : null;
    }
}
