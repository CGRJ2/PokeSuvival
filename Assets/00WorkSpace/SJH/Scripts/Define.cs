
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
	// TODO : 임시 데이터베이스
	private static bool _isInit;
	private static Dictionary<int, PokemonData> _numberToPokeData = new();
	private static Dictionary<string, PokemonData> _nameToPokeData = new();

	public static void PokeDataInit()
	{
		if (_isInit) return;

		PokemonData[] all = Resources.LoadAll<PokemonData>("PokemonSO");

		foreach (var data in all)
		{
			if (_numberToPokeData == null) _numberToPokeData = new();
			if (!_numberToPokeData.ContainsKey(data.PokeNumber)) _numberToPokeData.Add(data.PokeNumber, data);

			if (_nameToPokeData == null) _nameToPokeData = new();
			if (!_nameToPokeData.ContainsKey(data.PokeName)) _nameToPokeData.Add(data.PokeName, data);
		}
		Debug.Log($"PokemonData 초기화 {_numberToPokeData.Count} / {_nameToPokeData.Count}");
		_isInit = true;
	}

	public static PokemonData GetPokeData(int pokeNumber)
	{
		PokeDataInit();
		return _numberToPokeData.TryGetValue(pokeNumber, out var data) && data != null ? data : null;
	}
	public static PokemonData GetPokeData(string pokeName)
	{
		PokeDataInit();
		return _nameToPokeData.TryGetValue(pokeName, out var data) && data != null ? data : null;
	}
}
#region SJH
public enum PokemonType
{
	None,
	Normal,
	Fire,
	Water,
	Grass,
	Poison,
}
[Serializable]
public struct PokemonStat
{
	public int Hp;
	public int Attak;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;

	public float GetMoveSpeed() => Speed / 10f;
}
public enum SkillSlot
{
	Skill1,
	Skill2,
	Skill3,
	Skill4,
}
public enum SkillType
{
	Physical,
	Special,
	Status,
}
public enum AttackType
{
	Melee,
	Ranged,
}
#endregion
