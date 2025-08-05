using NTJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPassive", menuName = "ScriptableObjects/ItemPassive")]
public class ItemPassive : ItemData
{
    [Header("�нú� ���� ���ʽ�")]
    public List<StatBonus> statBonuses;

    [Header("�Ӽ� ������ ��ȭ")]
    public List<ElementalBoost> elementalBoosts; // �Ӽ� ��� ��ȭ

    private void OnEnable()
    {
        itemType = ItemType.Passive; // �ڵ����� Passive ����
    }
}
