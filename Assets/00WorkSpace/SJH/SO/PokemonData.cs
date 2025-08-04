using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Pokemon/PokemonData")]
public class PokemonData : ScriptableObject
{
	public int PokeNumber;
	public string PokeName;
	public PokemonType[] PokeTypes;
	public PokemonStat BaseStat;
	public RuntimeAnimatorController AnimController;
	public Sprite PokemonIconSprite; // 슬롯
	public Sprite PokemonInfoSprite; // 도감
	public PokemonSkill[] Skills;
	public string Desc;
	public int EvoLevel;
	public PokemonData NextEvoData;
	public bool IsCanEvo => NextEvoData != null;
	public float PokeSize;
}
