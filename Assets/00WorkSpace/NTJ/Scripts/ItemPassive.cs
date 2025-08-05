using NTJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPassive", menuName = "ScriptableObjects/ItemPassive")]
public class ItemPassive : ItemData
{
    [Header("패시브 스탯 보너스")]
    public List<StatBonus> statBonuses;

    [Header("속성 데미지 강화")]
    public List<ElementalBoost> elementalBoosts; // 속성 기술 강화

    private void OnEnable()
    {
        itemType = ItemType.Passive; // 자동으로 Passive 설정
    }
}
