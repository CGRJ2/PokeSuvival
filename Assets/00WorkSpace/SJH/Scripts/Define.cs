
using System;

public static class Define
{

}
#region PokemonInfo
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
	public int SpeicalDefense;
	public int Speed;

	public float GetMoveSpeed() => Speed / 10f;
}
#endregion
