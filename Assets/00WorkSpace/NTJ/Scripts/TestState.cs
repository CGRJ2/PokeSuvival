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

        // 버프 관리용 (StatType -> multiplier)
        private Dictionary<StatType, float> activeBuffs = new();
        private Dictionary<StatType, Coroutine> buffCoroutines = new();
        private Dictionary<StatType, float> buffEndTime = new();

        // 최종 스탯 계산용 (기본스탯 * 버프배율)
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
                    Debug.LogWarning($"정의되지 않은 아이템 타입: {item.itemType}");
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
            Debug.Log($"HP 회복: {value}, 현재 HP: {newHp}/{playerModel.MaxHp}");
        }

        private void LevelUp()
        {
            playerModel.SetLevel(playerModel.PokeLevel + 1);
        }

        private void ApplyBuff(StatType stat, float multiplier, float duration)
        {
            if (Mathf.Approximately(multiplier, 1f) || multiplier <= 0f)
            {
                Debug.LogWarning($"{stat} 비정상 배율 방지 : {multiplier}");
                return;
            }

            // 이미 해당 스탯에 버프가 존재한다면
            if (activeBuffs.ContainsKey(stat))
            {
                // 기존 코루틴 시간이 남아있다면 갱신 조건 확인
                if (buffCoroutines.TryGetValue(stat, out Coroutine coroutine))
                {
                    // 기존보다 새 duration이 더 길면 갱신
                    float remainingTime = GetRemainingBuffTime(stat);

                    if (duration > remainingTime)
                    {
                        StopCoroutine(coroutine);
                        buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
                        Debug.Log($"{stat} 버프 지속시간 갱신: {remainingTime:F1}s → {duration}s");
                    }
                    else
                    {
                        Debug.Log($"{stat} 버프 무시 (남은 시간 {remainingTime:F1}s > 새 버프 {duration}s)");
                    }
                }
                return; // 중첩 없음, 배율은 그대로 유지
            }

            // 신규 버프
            activeBuffs[stat] = multiplier;
            buffCoroutines[stat] = StartCoroutine(RemoveBuffAfterDelay(stat, multiplier, duration));
            Debug.Log($"{stat} 버프 시작, 배율: {multiplier}, 지속시간: {duration}s");
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
                    Debug.Log($"{stat} 버프 종료");
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
