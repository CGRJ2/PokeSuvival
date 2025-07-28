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
        public float value; // ������ �Ǵ� ȸ����
        public float duration; // ���� ���ӽð�

        public Sprite sprite;
    }
}