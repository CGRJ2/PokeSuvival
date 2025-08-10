using NTJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDropEntry
{
    public GameObject prefab; // 실제로 생성할 아이템 프리팹
    public ItemData itemData;  // 해당 아이템의 데이터
    public float dropProbability; // 드롭 확률
}
