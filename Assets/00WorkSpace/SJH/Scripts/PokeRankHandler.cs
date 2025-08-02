using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeRankHandler
{
	public Dictionary<StatType, int> RankUpdic { get; private set; }
	/// <summary>
	/// 스탯 타입, 이전 랭크, 이후 랭크
	/// </summary>
	public Action<StatType, int, int> OnRankChanged;

	public PokeRankHandler()
	{
		RankUpdic = new()
		{
			[StatType.Attack] = 0,
			[StatType.Defense] = 0,
			[StatType.SpAttack] = 0,
			[StatType.SpDefense] = 0,
			[StatType.Speed] = 0,
		};
	}

	public void SetRankUp(StatType statType, int value, float duration)
	{
		if (!RankUpdic.ContainsKey(statType)) return;

		int prevRank = RankUpdic[statType];
		int nextRank = Mathf.Clamp(prevRank + value, -6, 6);

		if (prevRank != nextRank)
		{
			RankUpdic[statType] = nextRank;
			OnRankChanged?.Invoke(statType, prevRank, nextRank);

			string text = value > 0 ? "상승" : "하락";
			Debug.Log($"{statType} 랭크 {text}: {prevRank} -> {nextRank}");
		}
		else if (value != 0)
		{
			string text = nextRank == 6 ? "올릴" : "내릴";
			Debug.Log($"더는 {statType} 랭크를 {text} 수 없음");
		}
	}

	public void ApplyToRank(PokemonStat allStat)
	{

	}
}