using NTJ;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Define
{
	// TODO : 임시 데이터베이스
	private static bool _isDataInit;
	public static Dictionary<int, PokemonData> NumberToPokeData { get; private set; }
	public static Dictionary<string, PokemonData> NameToPokeData { get; private set; }
	private static bool _isTypeInit;
	private const float Weak = 0.5f;
	private const float Strong = 2f;
	private const float NoDamage = 0f;
	private static Dictionary<PokemonType, Dictionary<PokemonType, float>> _pokeTypeChart = new();
	private static bool _isSkillInit;
	public  static Dictionary<string, PokemonSkill> PokeSkillDic { get; private set; }

	static void PokeDataInit()
	{
		if (_isDataInit) return;

		PokemonData[] all = Resources.LoadAll<PokemonData>("PokemonSO");

		foreach (var data in all)
		{
			if (NumberToPokeData == null) NumberToPokeData = new();
			if (!NumberToPokeData.ContainsKey(data.PokeNumber)) NumberToPokeData.Add(data.PokeNumber, data);

			if (NameToPokeData == null) NameToPokeData = new();
			if (!NameToPokeData.ContainsKey(data.PokeName)) NameToPokeData.Add(data.PokeName, data);
            //Debug.Log($"PokemonData 초기화 {NumberToPokeData.Count} / {all.Length} / {data.PokeName} /{data.name}");
        }
        //Debug.Log($"PokemonData 초기화 {NumberToPokeData.Count} / {all.Length}");
		_isDataInit = true;
	}
	public static PokemonData GetPokeData(int pokeNumber)
	{
		PokeDataInit();
		return NumberToPokeData.TryGetValue(pokeNumber, out var data) && data != null ? data : null;
	}
	public static PokemonData GetPokeData(string pokeName)
	{
		PokeDataInit();
		return NameToPokeData.TryGetValue(pokeName, out var data) && data != null ? data : null;
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

	static void PokeSkillInit()
	{
		if (_isSkillInit) return;

		PokemonSkill[] all = Resources.LoadAll<PokemonSkill>("PokemonSkillSO");

		foreach (var data in all)
		{
			if (PokeSkillDic == null) PokeSkillDic = new();
			if (!PokeSkillDic.ContainsKey(data.SkillName)) PokeSkillDic.Add(data.SkillName, data);
		}
		Debug.Log($"PokemonSkillData 초기화 {PokeSkillDic.Count}");
		_isSkillInit = true;
	}
	public static PokemonSkill GetPokeSkillData(string skillName)
	{
		PokeSkillInit();
		return PokeSkillDic.TryGetValue(skillName, out var data) ? data : null;
	}
    #region Item DB

    public static ItemDatabase ItemDatabase { get; private set; }
    [Header("아이템 DB")]
    public static ItemData[] items;
    private static Dictionary<int, ItemData> _itemDict;

    public static void ItemDatabaseInit()
    {
        ItemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
        _itemDict = ItemDatabase.items.ToDictionary(i => i.id);
        /*foreach (var kvp in _itemDict)
        {
            Debug.Log($"Key : {kvp.Key} / Value : {kvp.Value.itemName}");
        }
        Debug.Log($"아이템 데이터 {_itemDict.Count}개 초기화 완료");*/
    }

    public static ItemData GetItemById(int id)
    {
        if (ItemDatabase == null) ItemDatabaseInit();
        return _itemDict != null && _itemDict.TryGetValue(id, out var data) ? data : null;
    }

    #endregion
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
		// 종족값 최대 + 개체값 최대 + 노력치 최대
		// 100레벨 기준 최대 능력치 => 2B + IV + EV/4 + 5 = 510 + 31 + 63 + 5 = 609
		float baseSpeed = 4.5f;
        float bonusMax = 2.5f;    // 능력치 최대값일 때 추가될 수 있는 최대 속도 수치

        float maxAdditionalSpeed = bonusMax * 3f; // 6단계 랭크업일 때 적용될 수 있는 최대 속도 수치
        float normalizedValue = Mathf.Clamp01(Speed/(609f * 3f));
        float finalSpeed = baseSpeed + normalizedValue * maxAdditionalSpeed;
        return finalSpeed;
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

	public ItemPassive HeldItem;
	public List<StatusType> CurrentStatus;
	public List<string> CurrentBuffs;

	public BattleDataTable(int level, PokemonData pokeData, PokemonStat pokeStat, int maxHp, int currentHp,
		bool isAI = false, PlayerController pc = null, ItemPassive heldItem = null, List<StatusType> currentStatus = null, List<string> currentBuffs = null)
	{
		Level = level;
		PokeData = pokeData;
		AllStat = pokeStat;
		MaxHp = maxHp;
		CurrentHp = currentHp;

		IsAI = isAI;
		PC = pc;
		HeldItem = heldItem;
		CurrentStatus = currentStatus == null ? new List<StatusType>() { StatusType.None } : currentStatus;
		CurrentBuffs = currentBuffs == null ? new List<string>() : currentBuffs;
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
public enum StatusType
{
	None,		// 기본 상태
	// 상태이상
	Poison,		// 독
	Burn,		// 화상 : 물리 공격 반감
	Paralysis,	// 마비 : 스피드 반감
	Sleep,      // 수면 : 이동불가 회전불가
	Freeze,		// 동상	: 이동불가 회전불가
	// 상태변화
	Confusion,	// 혼란	: 키입력 반대로
	Binding,	// 속박 : 이동불가 회전가능 기술사용가능
	Flinch,		// 풀죽음 : ?
	// 제자리 스탑
	Stun,
}
#endregion
