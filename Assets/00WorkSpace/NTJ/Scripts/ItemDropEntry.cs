using NTJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDropEntry
{
    public GameObject prefab; // ������ ������ ������ ������
    public ItemData itemData;  // �ش� �������� ������
    public float dropProbability; // ��� Ȯ��
}
