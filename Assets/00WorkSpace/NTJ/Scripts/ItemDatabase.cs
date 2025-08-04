using NTJ;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;
    public ItemData GetItemById(int id)
    {
        return items.Find(item => item != null && item.id == id);
    }
}
