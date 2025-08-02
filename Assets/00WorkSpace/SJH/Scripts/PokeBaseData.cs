using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PokeBaseData
{
	[field: SerializeField] public PokemonData PokeData { get; protected set; }
	[field: SerializeField] public PokemonStat AllStat { get; protected set; }
	[SerializeField] protected int _pokeLevel;
	public virtual int PokeLevel
	{
		get => _pokeLevel;
		protected set => _pokeLevel = value;
	}
	[SerializeField] protected int _currentHp;
	public virtual int CurrentHp
	{
		get => _currentHp;
		protected set => _currentHp = value;
	}
	[field: SerializeField] public int MaxHp { get; protected set; }
	[field: SerializeField] public Dictionary<SkillSlot, float> SkillCooldownDic { get; protected set; }

	public bool IsDead { get; protected set; }

	public virtual float GetMoveSpeed() => PokeData.BaseStat.GetMoveSpeed();

	public virtual PokemonSkill GetSkill(int index)
	{
		var skills = PokeData.Skills;
		if (skills == null || skills.Length == 0 || index < 0 || index >= skills.Length) return null;
		return skills[index];
	}

	public abstract void SetCurrentHp(int hp);
	public abstract void SetLevel(int level);
	public virtual bool IsSkillCooldown(SkillSlot slot) => SkillCooldownDic.TryGetValue(slot, out var endTime) && Time.time < endTime;
	public virtual void SetSkillCooldown(SkillSlot slot, float cooldown) => SkillCooldownDic[slot] = Time.time + cooldown;
	protected virtual void OnDead() { }
}