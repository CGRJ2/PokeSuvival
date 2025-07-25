using System;
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
			_pokeLevel = value;
			OnPokeLevelChanged?.Invoke(value);
		}
	}
	public event Action<int> OnPokeLevelChanged;
	[field: SerializeField] public int PokeExp { get; private set; } = 0;
	[field: SerializeField] public int MaxHp { get; private set; }
	[SerializeField] private int _currentHp;
	public int CurrentHp
	{
		get => _currentHp;
		private set
		{
			_currentHp = value;
			OnCurrentHpChanged?.Invoke(value);
		}
	}
	public event Action<int> OnCurrentHpChanged;
	

	public bool IsMoving { get; private set; }

	// TODO : 패시브 아이템 리스트

	public PlayerModel(string playerName, PokemonData pokemonData, int level = 1, int exp = 0, int currentHp = -1)
    {
        PlayerName = playerName;
		PokeData = pokemonData;
		PokeLevel = level;
		PokeExp = exp;
		AllStat = new PokemonStat
		{
			Hp = PokeUtils.CalculateHp(level, pokemonData.BaseStat.Hp),
			Attak = PokeUtils.CalculateStat(level, pokemonData.BaseStat.Attak),
			Defense = PokeUtils.CalculateStat(level, pokemonData.BaseStat.Defense),
			SpecialAttack = PokeUtils.CalculateStat(level, pokemonData.BaseStat.SpecialAttack),
			SpecialDefense = PokeUtils.CalculateStat(level, pokemonData.BaseStat.SpecialDefense),
			Speed = PokeUtils.CalculateStat(level, pokemonData.BaseStat.Speed),
		};
		MaxHp = AllStat.Hp;
		if (currentHp == -1) CurrentHp = MaxHp;
		else CurrentHp = currentHp;
	}

	public float GetMoveSpeed() => PokeData.BaseStat.GetMoveSpeed();
	public void SetMoving(bool moving) => IsMoving = moving;
	public bool IsDead() => CurrentHp <= 0;
	public PokemonData GetNextEvoData() => PokeData.NextEvoData;
	public void SetCurrentHp(int hp) => _currentHp = hp;
	public PokemonSkill GetSkill(int index)
	{
		var skills = PokeData.Skills;

		if (skills == null || skills.Length == 0 || index < 0 || index >= skills.Length) return null;

		return skills[index];
	}
}
