using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{

    [CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
    public class ItemData : ScriptableObject
    {
        public int id;
        public string itemName;
        public ItemType itemType;
        public StatType affectedStat;
        public float value; // 증가량 또는 회복량
        public float duration; // 버프 지속시간
        public Sprite sprite;
        public string description; //아이템 설명
        public List<StatBonus> passiveStatBonuses; // 패시브 효과
        [Header("가격")]
        public int price;
    }
}