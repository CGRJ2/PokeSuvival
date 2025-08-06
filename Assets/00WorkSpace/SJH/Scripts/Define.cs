using System;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
	// TODO : 임시 데이터베이스
	private static bool _isDataInit;
	private static Dictionary<int, PokemonData> _numberToPokeData = new();
	private static Dictionary<string, PokemonData> _nameToPokeData = new();
	private static bool _isTypeInit;
	private const float Weak = 0.5f;
	private const float Strong = 2f;
	private const float NoDamage = 0f;
	private static Dictionary<PokemonType, Dictionary<PokemonType, float>> _pokeTypeChart = new();

	static void PokeDataInit()
	{
		if (_isDataInit) return;

		PokemonData[] all = Resources.LoadAll<PokemonData>("PokemonSO");

		foreach (var data in all)
		{
			if (_numberToPokeData == null) _numberToPokeData = new();
			if (!_numberToPokeData.ContainsKey(data.PokeNumber)) _numberToPokeData.Add(data.PokeNumber, data);

			if (_nameToPokeData == null) _nameToPokeData = new();
			if (!_nameToPokeData.ContainsKey(data.PokeName)) _nameToPokeData.Add(data.PokeName, data);
		}
		Debug.Log($"PokemonData 초기화 {_numberToPokeData.Count} / {_nameToPokeData.Count}");
		_isDataInit = true;
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

	static void PokeTypeInit()
	{
		if (_isTypeInit) return;
		_pokeTypeChart = new()
		{
			[PokemonType.Normal] = new()
			{
				[PokemonType.Rock] = Weak,
				[PokemonType.Ghost] = NoDamage,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Fire] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Water] = Weak,
				[PokemonType.Grass] = Strong,
				[PokemonType.Ice] = Strong,
				[PokemonType.Bug] = Strong,
				[PokemonType.Rock] = Weak,
				[PokemonType.Dragon] = Weak,
				[PokemonType.Steel] = Strong,
			},
			[PokemonType.Water] = new()
			{
				[PokemonType.Fire] = Strong,
				[PokemonType.Water] = Weak,
				[PokemonType.Grass] = Weak,
				[PokemonType.Ground] = Strong,
				[PokemonType.Rock] = Strong,
				[PokemonType.Dragon] = Weak,
			},
			[PokemonType.Grass] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Water] = Strong,
				[PokemonType.Grass] = Weak,
				[PokemonType.Poison] = Weak,
				[PokemonType.Ground] = Strong,
				[PokemonType.Flying] = Weak,
				[PokemonType.Bug] = Weak,
				[PokemonType.Rock] = Strong,
				[PokemonType.Dragon] = Weak,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Electric] = new()
			{
				[PokemonType.Water] = Strong,
				[PokemonType.Grass] = Weak,
				[PokemonType.Electric] = Weak,
				[PokemonType.Ground] = NoDamage,
				[PokemonType.Flying] = Strong,
				[PokemonType.Dragon] = Weak,
			},
			[PokemonType.Ice] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Water] = Weak,
				[PokemonType.Grass] = Strong,
				[PokemonType.Ice] = Weak,
				[PokemonType.Ground] = Strong,
				[PokemonType.Flying] = Strong,
				[PokemonType.Dragon] = Strong,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Fighting] = new()
			{
				[PokemonType.Normal] = Strong,
				[PokemonType.Ice] = Strong,
				[PokemonType.Poison] = Weak,
				[PokemonType.Flying] = Weak,
				[PokemonType.Psychic] = Weak,
				[PokemonType.Bug] = Weak,
				[PokemonType.Rock] = Strong,
				[PokemonType.Ghost] = NoDamage,
				[PokemonType.Dark] = Strong,
				[PokemonType.Steel] = Strong,
				[PokemonType.Fairy] = Weak,
			},
			[PokemonType.Poison] = new()
			{
				[PokemonType.Grass] = Strong,
				[PokemonType.Poison] = Weak,
				[PokemonType.Ground] = Weak,
				[PokemonType.Rock] = Weak,
				[PokemonType.Ghost] = Weak,
				[PokemonType.Steel] = NoDamage,
				[PokemonType.Fairy] = Strong,
			},
			[PokemonType.Ground] = new()
			{
				[PokemonType.Fire] = Strong,
				[PokemonType.Grass] = Weak,
				[PokemonType.Electric] = Strong,
				[PokemonType.Poison] = Strong,
				[PokemonType.Flying] = NoDamage,
				[PokemonType.Bug] = Weak,
				[PokemonType.Rock] = Strong,
				[PokemonType.Steel] = Strong,
			},
			[PokemonType.Flying] = new()
			{
				[PokemonType.Grass] = Strong,
				[PokemonType.Electric] = Weak,
				[PokemonType.Fighting] = Strong,
				[PokemonType.Bug] = Strong,
				[PokemonType.Rock] = Weak,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Psychic] = new()
			{
				[PokemonType.Fighting] = Strong,
				[PokemonType.Poison] = Strong,
				[PokemonType.Psychic] = Weak,
				[PokemonType.Dark] = NoDamage,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Bug] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Grass] = Strong,
				[PokemonType.Fighting] = Weak,
				[PokemonType.Poison] = Weak,
				[PokemonType.Flying] = Weak,
				[PokemonType.Psychic] = Strong,
				[PokemonType.Ghost] = Weak,
				[PokemonType.Dark] = Strong,
				[PokemonType.Steel] = Weak,
				[PokemonType.Fairy] = Weak,
			},
			[PokemonType.Rock] = new()
			{
				[PokemonType.Fire] = Strong,
				[PokemonType.Ice] = Strong,
				[PokemonType.Fighting] = Weak,
				[PokemonType.Ground] = Weak,
				[PokemonType.Flying] = Strong,
				[PokemonType.Bug] = Strong,
				[PokemonType.Steel] = Weak,
			},
			[PokemonType.Ghost] = new()
			{
				[PokemonType.Normal] = NoDamage,
				[PokemonType.Psychic] = Strong,
				[PokemonType.Ghost] = Strong,
				[PokemonType.Dark] = Weak,
			},
			[PokemonType.Dragon] = new()
			{
				[PokemonType.Dragon] = Strong,
				[PokemonType.Steel] = Weak,
				[PokemonType.Fairy] = NoDamage,
			},
			[PokemonType.Dark] = new()
			{
				[PokemonType.Fighting] = Weak,
				[PokemonType.Psychic] = Strong,
				[PokemonType.Ghost] = Strong,
				[PokemonType.Dark] = Weak,
				[PokemonType.Fairy] = Weak,
			},
			[PokemonType.Steel] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Water] = Weak,
				[PokemonType.Electric] = Weak,
				[PokemonType.Ice] = Strong,
				[PokemonType.Rock] = Strong,
				[PokemonType.Steel] = Weak,
				[PokemonType.Fairy] = Strong,
			},
			[PokemonType.Fairy] = new()
			{
				[PokemonType.Fire] = Weak,
				[PokemonType.Fighting] = Strong,
				[PokemonType.Poison] = Weak,
				[PokemonType.Dragon] = Strong,
				[PokemonType.Dark] = Strong,
				[PokemonType.Steel] = Weak,
			},
		};
		_isTypeInit = true;
	}
	public static float GetTypeDamageValue(PokemonType attackType, PokemonType defenderType)
	{
		PokeTypeInit();
		return _pokeTypeChart.TryGetValue(attackType, out var chart) && chart.TryGetValue(defenderType, out float result) ? result : 1f;
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
	Fighting,
	Electric,
	Ice,
	Ground,
	Flying,
	Psychic,
	Bug,
	Rock,
	Ghost,
	Dragon,
	Dark,
	Steel,
	Fairy,
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

	public float GetMoveSpeed()
	{
		// TODO : 이속 보정줘야하나 종족값 너무 낮으면 최소치, 너무빠르면 제한걸던가
		return Speed / 10f;
	}
	public int GetBaseStat() => Hp + Attak + Defense + SpecialAttack + SpecialDefense + Speed;
	public bool IsEqual(PokemonStat stat)
	{
		return Hp == stat.Hp &&
			   Attak == stat.Attak &&
			   Defense == stat.Defense &&
			   SpecialAttack == stat.SpecialAttack &&
			   SpecialDefense == stat.SpecialDefense &&
			   Speed == stat.Speed;
	}
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
[Serializable]
public struct BattleDataTable
{
	public int Level;
	public PokemonData PokeData;
	public PokemonStat AllStat;
	public int MaxHp;
	public int CurrentHp;

	public bool IsAI;
	public PlayerController PC;

	// TODO : 상태이상
	// TODO : 아이템 장착

	public BattleDataTable(int level, PokemonData pokeData, PokemonStat pokeStat, int maxHp, int currentHp, bool isAI = false, PlayerController pc = null)
	{
		Level = level;
		PokeData = pokeData;
		AllStat = pokeStat;
		MaxHp = maxHp;
		CurrentHp = currentHp;
		IsAI = isAI;
		PC = pc;
	}

	public bool IsVaild() => PokeData != null;
}
public enum SkillAnimType
{
	Attack,
	SpeAttack,
}
public enum AIState
{
	None,
	Idle,
	Move,
	Attack,
	Die,
}
public enum ItemType
{
	Buff,		// 도구
	Heal,		// 회복약
	LevelUp,	// 이상한사탕
	StatBuff,   // 스탯 상승 도구
    Passive     // 패시브 장비
}
public enum StatType
{
	HP,			// 체력은 없음
	Attack,		// 공격
	Defense,	// 방어
	SpAttack,	// 특공
	SpDefense,	// 특방
	Speed		// 스피드
}
public struct StatBonus
{
	public StatType statType;
	public float value;
}
#endregion
