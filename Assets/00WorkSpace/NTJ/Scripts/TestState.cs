using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
    public class TestState : MonoBehaviourPun, IStatReceiver
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerModel playerModel;

        // ���� ������ (StatType -> multiplier)
        private Dictionary<StatType, float> activeBuffs = new();
        private Dictionary<StatType, Coroutine> buffCoroutines = new();
        private Dictionary<StatType, float> buffEndTime = new();

        // ���� ���� ���� (�⺻���� * ��������)
        public float atk => playerModel.AllStat.Attak * GetBuffMultiplier(StatType.Atk);
        public float def => playerModel.AllStat.Defense * GetBuffMultiplier(StatType.Def);
        public float spA => playerModel.AllStat.SpecialAttack * GetBuffMultiplier(StatType.SpA);
        public float spD => playerModel.AllStat.SpecialDefense * GetBuffMultiplier(StatType.SpD);
        public float spe => playerModel.AllStat.Speed * GetBuffMultiplier(StatType.Spe);
        
        
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

                default:
                    Debug.LogWarning($"���ǵ��� ���� ������ Ÿ��: {item.itemType}");
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
            int newHp = Mathf.Min(playerModel.CurrentHp + (int)value, playerModel.MaxHp);
            playerModel.SetCurrentHp(newHp);
            Debug.Log($"HP ȸ��: {value}, ���� HP: {newHp}/{playerModel.MaxHp}");
        }

        private void LevelUp()
        {
            playerModel.SetLevel(playerModel.PokeLevel + 1);
            playerModel.SetExp(0);
            Debug.Log($"������! ���� ����: {playerModel.PokeLevel}, ����ġ �ʱ�ȭ");
        }

       // PlayerModel�� �߰��ؾ� ��

       // public void SetExp(int exp)
       // {
       //     _pokeExp = exp;
       //     _nextExp = PokeUtils.GetNextLevelExp(PokeLevel);
       // }

        private void ApplyBuff(StatType stat, float multiplier, float duration)
        {
            if (Mathf.Approximately(multiplier, 1f) || multiplier <= 0f)
            {
                Debug.LogWarning($"{stat} ������ ���� ���� : {multiplier}");
                return;
            }

            // �̹� �ش� ���ȿ� ������ �����Ѵٸ�
            if (activeBuffs.ContainsKey(stat))
            {
                // ���� �ڷ�ƾ �ð��� �����ִٸ� ���� ���� Ȯ��
                if (buffCoroutines.TryGetValue(stat, out Coroutine coroutine))
                {
                    // �������� �� duration�� �� ��� ����
                    float remainingTime = GetRemainingBuffTime(stat);

                    if (duration > remainingTime)
                    {
                        StopCoroutine(coroutine);
                        buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
                        Debug.Log($"{stat} ���� ���ӽð� ����: {remainingTime:F1}s �� {duration}s");
                    }
                    else
                    {
                        Debug.Log($"{stat} ���� ���� (���� �ð� {remainingTime:F1}s > �� ���� {duration}s)");
                    }
                }
                return; // ��ø ����, ������ �״�� ����
            }

            // �ű� ����
            activeBuffs[stat] = multiplier;
            buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
            Debug.Log($"{stat} ���� ����, ����: {multiplier}, ���ӽð�: {duration}s");
        }
        

        private IEnumerator RemoveBuffAfterDelay(StatType stat, float multiplier, float duration)
        {
            buffEndTime[stat] = Time.time + duration;
            yield return new WaitForSeconds(duration);

            RemoveBuff(stat, multiplier);
            buffEndTime.Remove(stat);
        }

        private void RemoveBuff(StatType stat, float multiplier)
        {
            if (activeBuffs.TryGetValue(stat, out float currentMultiplier))
            {
                if (Mathf.Approximately(currentMultiplier, multiplier))
                {
                    activeBuffs.Remove(stat);
                    Debug.Log($"{stat} ���� ����");
                }
            }
        }

        private float GetBuffMultiplier(StatType stat)
        {
            if (activeBuffs.TryGetValue(stat, out float multiplier))
                return multiplier;
            return 1f;
        }
        private float GetRemainingBuffTime(StatType stat)
        {
            if (buffEndTime.TryGetValue(stat, out float endTime))
            {
                return Mathf.Max(0, endTime - Time.time);
            }
            return 0;
        }
    }
}