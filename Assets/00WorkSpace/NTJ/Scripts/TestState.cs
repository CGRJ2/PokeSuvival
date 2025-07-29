using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
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
        private Dictionary<StatType, Coroutine> buffCoroutines = new();

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

            ReapplyBuffs();
        }

        private void ApplyBuff(StatType stat, float multiplier, float duration)
        {
            if (Mathf.Approximately(multiplier, 1f) || multiplier <= 0f)
            {
                Debug.LogWarning($"{stat} ������ ���� ���� : {multiplier}");
                return;
            }

            if (activeBuffs.ContainsKey(stat))
            {
                Debug.Log($"{stat} ���� ���ӽð� �ʱ�ȭ");

                if (buffCoroutines.TryGetValue(stat, out Coroutine coroutine))
                {
                    StopCoroutine(coroutine);
                }

                buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
                return;
            }

            ApplyMultiplier(stat, multiplier);
            activeBuffs[stat] = multiplier;
            buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
        }

        private IEnumerator RemoveBuffAfterDelay(StatType stat, float multiplier, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveBuff(stat, multiplier);
        }

        private void RemoveBuff(StatType stat, float multiplier)
        {
            if (activeBuffs.TryGetValue(stat, out float currentMultiplier))
            {
                if (Mathf.Approximately(currentMultiplier, multiplier))
                {
                    ApplyMultiplier(stat, 1f / multiplier);
                    activeBuffs.Remove(stat);
                    Debug.Log($"{stat} ���� ���� (���� {multiplier}�� �� 1.0)");
                }
            }
        }

        private void ApplyMultiplier(StatType stat, float multiplier)
        {
            switch (stat)
            {
                case StatType.Atk: atk *= multiplier; break;
                case StatType.Def: def *= multiplier; break;
                case StatType.SpA: spA *= multiplier; break;
                case StatType.SpD: spD *= multiplier; break;
                case StatType.Spe: spe *= multiplier; break;
            }
        }
        private void ReapplyBuffs()
        {
            foreach (var buffEntry in activeBuffs)
            {
                StatType stat = buffEntry.Key;
                float multiplier = buffEntry.Value;

                ApplyMultiplier(stat, multiplier);
                Debug.Log($"{stat} ���� ������ (x{multiplier})");
            }
        }
    }
}