using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
    [CreateAssetMenu(fileName = "PokemonData", menuName = "ScriptableObjects/PokemonData")]
    public class TestState : MonoBehaviourPun, IStatReceiver
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PokemonData pokemonData;

        // ���� ���� ���� ���Ǵ� ���� ���� ��
        public int level = 1;
        public float currentHP;
        public float maxHP;

        public float atk;
        public float def;
        public float spA;
        public float spD;
        public float spe;

        // ���� ������ (��ø ���� ���� & RemoveStat �� �ʿ�)
        private Dictionary<StatType, float> activeBuffs = new();

        public void ApplyStat(ItemData item)
        {
            switch (item.itemType)
            {
                case ItemType.Heal:
                    Heal(item.value);
                    break;

                case ItemType.LevelUp:
                    LevelUp();
                    break;

                case ItemType.Buff:
                    ApplyBuff(item.affectedStat, item.value, item.duration);
                    break;
            }
        }

        public void RemoveStat(ItemData item)
        {
            if (item.itemType == ItemType.Buff)
            {
                RemoveBuff(item.affectedStat, item.value);
            }
        }

        private void Heal(float value)
        {
            currentHP = Mathf.Min(currentHP + value, maxHP);
            Debug.Log($"HP ȸ��: {value}, ���� HP: {currentHP}/{maxHP}");
        }

        private void LevelUp()
        {
            // ������ �� �ɷ�ġ ���� ����
            level++;
            maxHP += 10;
            currentHP = maxHP;
            atk += 1;
            def += 1;
            spA += 1;
            spD += 1;
            spe += 1;

            //  Sprite ����
            if (pokemonData.levelSprites.Length > level - 1)
                spriteRenderer.sprite = pokemonData.levelSprites[level - 1];

            Debug.Log($"������! ���� ����: {level}");
        }

        private void ApplyBuff(StatType stat, float value, float duration)
        {
            if (activeBuffs.ContainsKey(stat))
            {
                Debug.Log($"{stat} ��ø �Ұ�");
                return; // ��ø ����
            }

            ModifyStat(stat, value);
            activeBuffs[stat] = value;

            // ���� �ð� �ڿ� ���� ����
            StartCoroutine(RemoveBuffAfterDelay(stat, value, duration));
        }

        private IEnumerator RemoveBuffAfterDelay(StatType stat, float value, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveBuff(stat, value);
        }

        private void RemoveBuff(StatType stat, float value)
        {
            if (activeBuffs.TryGetValue(stat, out float current))
            {
                float actualValue = Mathf.Min(current, value);
                ModifyStat(stat, -actualValue);
                activeBuffs[stat] -= actualValue;

                if (activeBuffs[stat] <= 0)
                    activeBuffs.Remove(stat);

                Debug.Log($"{stat} -{actualValue} (���� ����)");
            }
        }

        private void ModifyStat(StatType stat, float value)
        {
            switch (stat)
            {
                case StatType.HP:
                    maxHP += value;
                    currentHP += value;
                    break;
                case StatType.Atk: atk += value; break;
                case StatType.Def: def += value; break;
                case StatType.SpA: spA += value; break;
                case StatType.SpD: spD += value; break;
                case StatType.Spe: spe += value; break;
            }
        }
    }
}