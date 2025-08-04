using NTJ;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items;
}
