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
	// TODO : 스킬 리스트
	public string Desc;
	public int MaxHp;
	public int Hp;
	public int EvoLevel; // TODO : 진화 기준 변경
	public PokemonData NextEvoData;
	public bool IsCanEvo => NextEvoData != null;
	// TODO : 패시브
}
