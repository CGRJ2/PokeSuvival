using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerModel
{
	[field: SerializeField] public string PlayerName { get; private set; }
    [field: SerializeField] public PokemonData PokeData { get; private set; }
	public PokemonStat AllStat;
	[SerializeField] private int _pokeLevel;
	public int PokeLevel
	{
		get => _pokeLevel;
		private set
		{
			if (_pokeLevel == value) return;
			Debug.Log($"레벨 변경 {_pokeLevel} => {value}");
			_pokeLevel = value;
			OnPokeLevelChanged?.Invoke(value);
			ReCalculateAllStat();
		}
	}
	public Action<int> OnPokeLevelChanged;
	[SerializeField] private int _pokeExp;
	public int PokeExp
	{
		get => _pokeExp;
		set
		{
			Debug.Log($"{_pokeExp} + {value} = {_pokeExp + value}");
			_pokeExp += value;
			while (true)
			{
				int requiredExp = PokeUtils.GetNextLevelExp(PokeLevel);
				if (_pokeExp >= requiredExp)
				{
					_pokeExp -= requiredExp;
					PokeLevel++;
					Debug.Log($"레벨업! 현재 레벨: {PokeLevel}");
				}
				else break;
			}
			_nextExp = PokeUtils.GetNextLevelExp(PokeLevel);
		}
	}
	[SerializeField] private int _nextExp;
	[field: SerializeField] public int MaxHp { get; private set; }
	[SerializeField] private int _currentHp;
	public int CurrentHp
	{
		get => _currentHp;
		private set
		{
			if (_currentHp == value) return;
			Debug.Log($"체력 변경 {_currentHp} => {value}");
			_currentHp = value;
			OnCurrentHpChanged?.Invoke(value);

			if (_currentHp <= 0 && !IsDead)
			{
				IsDead = true;
				OnDied?.Invoke();
			}
		}
	}
	public Action<int> OnCurrentHpChanged;
	public Action OnDied;
	
	public bool IsMoving { get; private set; }
	public bool IsDead { get; private set; }

	private Dictionary<SkillSlot, float> _skillCooldownDic;

	// TODO : 패시브 아이템 리스트

	public PlayerModel(string playerName, PokemonData pokemonData, int level = 1, int exp = 0, int currentHp = -1)
    {
        PlayerName = playerName;
		PokeData = pokemonData;
		PokeLevel = level;
		PokeExp = exp;
		AllStat = PokeUtils.CalculateAllStat(level, pokemonData.BaseStat);
		MaxHp = AllStat.Hp;
		if (currentHp == -1) CurrentHp = MaxHp;
		else CurrentHp = currentHp;
		_skillCooldownDic = new()
		{
			[SkillSlot.Skill1] = 0,
			[SkillSlot.Skill2] = 0,
			[SkillSlot.Skill3] = 0,
			[SkillSlot.Skill4] = 0,
		};
	}

	public float GetMoveSpeed() => PokeData.BaseStat.GetMoveSpeed();
	public void SetMoving(bool moving) => IsMoving = moving;
	public PokemonData GetNextEvoData() => PokeData.NextEvoData;
	public void SetCurrentHp(int hp) => CurrentHp = hp;
	public void SetLevel(int level) => PokeLevel = level;
	public void SetLocalLevel(int level) => _pokeLevel = level;
	public void AddExp(int exp) => PokeExp = _pokeExp + exp;
	public PokemonSkill GetSkill(int index)
	{
		var skills = PokeData.Skills;

		if (skills == null || skills.Length == 0 || index < 0 || index >= skills.Length) return null;

		return skills[index];
	}
	public bool IsSkillCooldown(SkillSlot slot, float cooldown) => _skillCooldownDic.TryGetValue(slot, out var lastCooldown) && Time.time - lastCooldown < cooldown;
	public void SetSkillCooldown(SkillSlot slot) => _skillCooldownDic[slot] = Time.time;
	public void ReCalculateAllStat()
	{
		int hpGap = MaxHp - _currentHp;
		AllStat = PokeUtils.CalculateAllStat(_pokeLevel, PokeData.BaseStat);
		MaxHp = AllStat.Hp;
		_currentHp = Mathf.Min(MaxHp - hpGap, MaxHp);
	}
	public void PokemonEvolution(PokemonData nextData)
	{
		string prevName = PokeData.PokeName;
		int hpGap = MaxHp - _currentHp;
		PokeData = nextData;
		AllStat = PokeUtils.CalculateAllStat(PokeLevel, PokeData.BaseStat);
		MaxHp = AllStat.Hp;
		_currentHp = Mathf.Min(MaxHp - hpGap, MaxHp);
		Debug.Log($"포켓몬 진화 : {prevName} -> {PokeData.PokeName}");
	}

}
