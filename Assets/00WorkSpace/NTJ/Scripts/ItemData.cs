using UnityEngine;

namespace NTJ
{
    public enum ItemType { Buff, Heal, LevelUp }
    public enum StatType { HP, Atk, Def, SpA, SpD, Spe }

    [CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public ItemType itemType;
        public StatType affectedStat;
        public float value; // 증가량 또는 회복량
        public float duration; // 버프 지속시간

        public Sprite sprite;
    }
}