using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerModel : PokeBaseData
{
	[field: SerializeField] public string PlayerName { get; private set; }
	[field: SerializeField] public PokemonData PokeData { get; private set; }
	[field: SerializeField] public PokemonStat AllStat { get; private set; }
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
	public int PokeExp { get => _pokeExp; }
	[field: SerializeField] public int NextExp { get; private set; }
	[field: SerializeField] public int TotalExp { get; private set; }
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

	public Dictionary<SkillSlot, float> SkillCooldownDic { get; private set; }

	// TODO : 패시브 아이템 리스트

	public PlayerModel(string playerName, PokemonData pokemonData, int level = 1, int exp = 0, int currentHp = -1)
	{
		PlayerName = playerName;
		PokeData = pokemonData;
		PokeLevel = level;
		_pokeExp = exp;
		AllStat = PokeUtils.CalculateAllStat(level, pokemonData.BaseStat);
		MaxHp = AllStat.Hp;
		if (currentHp == -1) CurrentHp = MaxHp;
		else CurrentHp = currentHp;
		SkillCooldownDic = new()
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
	public void AddExp(int value)
	{
		Debug.Log($"{_pokeExp} + {value} = {_pokeExp + value}");

		_pokeExp += value;
		TotalExp += value;

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
		NextExp = PokeUtils.GetNextLevelExp(PokeLevel);
	}

	public PokemonSkill GetSkill(int index)
	{
		var skills = PokeData.Skills;

		if (skills == null || skills.Length == 0 || index < 0 || index >= skills.Length) return null;

		return skills[index];
	}
	public bool IsSkillCooldown(SkillSlot slot) => SkillCooldownDic.TryGetValue(slot, out var endTime) && Time.time < endTime;
	public void SetSkillCooldown(SkillSlot slot, float cooldown)
	{
		SkillCooldownDic[slot] = Time.time + cooldown;
		UIManager.Instance.InGameGroup.UpdateCoolTime(PlayerManager.Instance?.LocalPlayerController.Model, slot);
	}
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
	public void SetHeal(int value)
	{
		if (IsDead || value <= 0) return;

		int newHp = Mathf.Min(_currentHp + value, MaxHp);
		CurrentHp = newHp;

		Debug.Log($"{value} 만큼 회복! 현재 체력 : {_currentHp}");
	}
}
