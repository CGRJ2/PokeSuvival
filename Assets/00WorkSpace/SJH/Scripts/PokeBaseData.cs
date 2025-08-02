using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PokeBaseData
{
	[field: SerializeField] public PokemonData PokeData { get; protected set; }
	[field: SerializeField] public PokemonStat AllStat { get; protected set; }
	[field: SerializeField] public int PokeLevel { get; protected set; }
	[field: SerializeField] public int CurrentHp { get; protected set; }
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

	public virtual void SetCurrentHp(int hp)
	{
		CurrentHp = hp;
		if (CurrentHp <= 0 && !IsDead)
		{
			IsDead = true;
			OnDead();
		}
	}

	public virtual void SetLevel(int level)
	{
		PokeLevel = level;
	}

	public virtual bool IsSkillCooldown(SkillSlot slot) => SkillCooldownDic.TryGetValue(slot, out var endTime) && Time.time < endTime;
	public virtual void SetSkillCooldown(SkillSlot slot, float cooldown) => SkillCooldownDic[slot] = Time.time + cooldown;
	protected abstract void OnDead();
}