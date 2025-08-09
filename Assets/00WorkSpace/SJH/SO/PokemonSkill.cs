using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/PokemonSkill")]
public class PokemonSkill : ScriptableObject
{
    public string SkillName;
    public string Desc;

    public int Damage;
    public float Cooldown;
    public float Range;
    public PokemonType PokeType;
    public SkillType SkillType;
    public SkillAnimType SkillAnimType;

	public GameObject EffectPrefab;

    public StatusType StatusEffect;
    public Sprite StatusSprite;
    public float StatusRate; // 0 ~ 1f;
    public float StatusDuration;
}
