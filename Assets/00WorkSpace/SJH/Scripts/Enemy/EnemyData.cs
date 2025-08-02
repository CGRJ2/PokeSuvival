using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyData
{
	private Enemy _enemy;
	[field: SerializeField] public string PokeName { get; private set; }
	[field: SerializeField] public PokemonData PokeData { get; private set; }
	[field: SerializeField] public PokemonStat AllStat { get; private set; }
	[field: SerializeField] public int PokeLevel { get; private set; }
	[field: SerializeField] public int CurrentHp { get; private set; }
	[field: SerializeField] public int MaxHp { get; private set; }
	[field: SerializeField] public Dictionary<SkillSlot, float> SkillCooldownDic { get; private set; }
	public bool IsDead { get; private set; }

	public EnemyData(Enemy enemy, string pokeName, PokemonData pokeData, int level = 1, int currentHp = -1)
	{
		_enemy = enemy;
		PokeName = pokeName;
		PokeData = pokeData;
		PokeLevel = level;
		CurrentHp = currentHp;
		AllStat = PokeUtils.CalculateAllStat(level, pokeData.BaseStat);
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
	public PokemonSkill GetSkill(int index)
	{
		var skills = PokeData.Skills;

		if (skills == null || skills.Length == 0 || index < 0 || index >= skills.Length) return null;

		return skills[index];
	}
	public void SetCurrentHp(int hp)
	{
		CurrentHp = hp;
		if (CurrentHp <= 0 && !IsDead)
		{
			IsDead = true;
			_enemy.photonView.RPC(nameof(_enemy.RPC_EnemyDead), RpcTarget.AllBuffered, GetDeathExp());
		}
	}
	public void SetLevel(int level) => PokeLevel = level;
	public bool IsSkillCooldown(SkillSlot slot) => SkillCooldownDic.TryGetValue(slot, out var endTime) && Time.time < endTime;
	public void SetSkillCooldown(SkillSlot slot, float cooldown) => SkillCooldownDic[slot] = Time.time + cooldown;
	public int GetDeathExp()
	{
		// 사망 경험치
		// 기본 밸류
		float baseValue = 10f;

		// 종족값
		int totalBaseStat = PokeData.BaseStat.GetBaseStat();

		// 가중치
		float modifyValue = totalBaseStat / 600f; // 600족기준

		return Mathf.RoundToInt(PokeLevel * baseValue * (1f + modifyValue));
	}
}
