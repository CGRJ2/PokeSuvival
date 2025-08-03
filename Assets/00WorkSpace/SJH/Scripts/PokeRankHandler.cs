using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class StatRank
{
	public StatType StatType;
	public int Rank;
	public StatRank(StatType statType, int rank)
	{
		StatType = statType;
		Rank = rank;
	}
}

[Serializable]
public class PokeRankHandler
{
	MonoBehaviour _routineClass; // PlayerController, Enemy
	public PokeBaseData BaseData { get; private set; }

	public Dictionary<StatType, int> RankUpdic { get; private set; }
	public Dictionary<StatType, float> RankEndTimes { get; private set; }
	public Dictionary<StatType, Coroutine> RankRoutines { get; private set; }

	public Action<StatType, int, int> OnRankChanged;	// 스탯종류, 이전값, 이후값
	public Action<StatType, int> OnSyncToRank;			// 스탯종류, 최신값

	[SerializeField] private List<StatRank> _rankDebug = new();

	public PokeRankHandler(MonoBehaviour routineClass, PokeBaseData baseData)
	{
		Debug.Log($"Rank 생성자 호출 baseData : {baseData}");
		_routineClass = routineClass;
		BaseData = baseData;
		RankUpdic = new()
		{
			[StatType.Attack] = 0,
			[StatType.Defense] = 0,
			[StatType.SpAttack] = 0,
			[StatType.SpDefense] = 0,
			[StatType.Speed] = 0,
		};
		RankEndTimes = new();
		RankRoutines = new();
	}

	public void SetRank(StatType statType, int value, float duration)
	{
		if (!RankUpdic.ContainsKey(statType)) return;

		int prevRank = RankUpdic[statType];
		int nextRank = Mathf.Clamp(prevRank + value, -6, 6);

		float nowTime = Time.time;

		// 중복일 경우 시간 중첩 or 추가
		if (RankEndTimes.TryGetValue(statType, out float prevEndTime)) RankEndTimes[statType] = Mathf.Max(prevEndTime, nowTime) + duration;
		else RankEndTimes[statType] = nowTime + duration;

		// 랭크 적용
		if (prevRank != nextRank)
		{
			RankUpdic[statType] = nextRank;
			OnRankChanged?.Invoke(statType, prevRank, nextRank);
			OnSyncToRank?.Invoke(statType, nextRank);

			string text = value > 0 ? "상승" : "하락";
			Debug.Log($"{statType} 랭크 {text}: {prevRank} -> {nextRank}");
		}
		else if (value != 0)
		{
			string text = nextRank == 6 ? "올릴" : "내릴";
			Debug.Log($"더는 {statType} 랭크를 {text} 수 없음");
		}

		// 지속시간 코루틴 실행
		if (RankRoutines.TryGetValue(statType, out var routine)) _routineClass.StopCoroutine(routine);
		RankRoutines[statType] = _routineClass.StartCoroutine(RankDurationRoutine(statType));

		RankDebug();
	}

	IEnumerator RankDurationRoutine(StatType statType)
	{
		float waitTime = RankEndTimes[statType] - Time.time;

		if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

		int prev = RankUpdic[statType];
		if (prev != 0)
		{
			RankUpdic[statType] = 0;
			OnRankChanged?.Invoke(statType, prev, 0);
			OnSyncToRank?.Invoke(statType, 0);
			Debug.Log($"{statType} 랭크 만료: {prev} > 0");
		}
		RankEndTimes.Remove(statType);
		RankRoutines.Remove(statType);
	}

	public void RankAllClear()
	{
		// 지속시간 코루틴 초기화
		foreach (var pair in RankRoutines)
		{
			if (pair.Value != null) _routineClass.StopCoroutine(pair.Value);
		}

		// 랭크업 초기화
		foreach (var statType in RankUpdic.Keys.ToList())
		{
			int prev = RankUpdic[statType];
			if (prev != 0)
			{
				RankUpdic[statType] = 0;
				OnRankChanged?.Invoke(statType, prev, 0);
				OnSyncToRank?.Invoke(statType, 0);
				Debug.Log($"{statType} 랭크 초기화: {prev} > 0");
			}
		}

		RankEndTimes.Clear();
		RankRoutines.Clear();

		RankDebug();
		Debug.Log("모든 랭크 초기화");
	}

	public PokemonStat GetRankedStat()
	{
		var baseStat = BaseData.AllStat;
		return new PokemonStat
		{
			Hp = baseStat.Hp,
			Attak = ApplyToRank(baseStat.Attak, RankUpdic[StatType.Attack]),
			Defense = ApplyToRank(baseStat.Defense, RankUpdic[StatType.Defense]),
			SpecialAttack = ApplyToRank(baseStat.SpecialAttack, RankUpdic[StatType.SpAttack]),
			SpecialDefense = ApplyToRank(baseStat.SpecialDefense, RankUpdic[StatType.SpDefense]),
			Speed = ApplyToRank(baseStat.Speed, RankUpdic[StatType.Speed]),
		};
	}

	int ApplyToRank(int baseValue, int rank)
	{
		if (rank > 0) return (int)(baseValue * (2f + rank) / 2f);
		if (rank < 0) return (int)(baseValue * 2f / (2f - rank));
		return baseValue;
	}

	void RankDebug()
	{
		_rankDebug = new();
		foreach (var kvp in RankUpdic)
		{
			StatRank statRank = new StatRank(kvp.Key, kvp.Value);
			_rankDebug.Add(statRank);
		}
	}

	public void RankSync(StatType statType, int rank)
	{
		if (!RankUpdic.ContainsKey(statType)) return;
		RankUpdic[statType] = rank;
		RankDebug();
		Debug.Log($"{BaseData.PokeData.PokeName} : [{statType} : {rank}] 동기화 완료");
	}
}