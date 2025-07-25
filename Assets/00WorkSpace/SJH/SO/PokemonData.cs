using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/PokemonData")]
public class PokemonData : ScriptableObject
{
	public int PokeNumber;
	public string PokeName;
	public PokemonType[] Types;
	public PokemonStat BaseStat;
	public RuntimeAnimatorController AnimController;
	public Sprite PokemonSprite;
	public PokemonSkill[] Skills;
	public string Desc;
	public int EvoLevel;
	public PokemonData NextEvoData;
	public bool IsCanEvo => NextEvoData != null;
	// TODO : 패시브
}
