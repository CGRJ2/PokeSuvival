using System;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    // TODO : 포켓몬에 따라 스탯 달라짐
    public string PlayerName { get; private set; }
    public PokemonData PokeData { get; private set; }
	public int MaxHp { get; private set; }
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
	public int PokeLevel { get; private set; } = 1;
	public int PokeExp { get; private set; } = 0;
	public bool IsMoving { get; private set; }

	public event Action<int> OnCurrentHpChanged;

	public PlayerModel(string playerName, PokemonData pokemonData)
    {
        PlayerName = playerName;
		PokeData = pokemonData;
	}

	public float GetMoveSpeed() => PokeData.BaseStat.GetMoveSpeed();
	public void SetMoving(bool moving) => IsMoving = moving;
	public bool IsDead() => CurrentHp <= 0;
	public PokemonData GetNextEvoData() => PokeData.NextEvoData;
	public void SetLevel(int level) => PokeLevel = level;
	public void SetExperience(int exp) => PokeExp = exp;
}
