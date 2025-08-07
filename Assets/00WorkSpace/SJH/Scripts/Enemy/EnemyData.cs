using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyData : PokeBaseData
{
	private Enemy _enemy;

	public EnemyData(Enemy enemy, PokemonData pokeData, int level = 1, int currentHp = -1)
	{
		_enemy = enemy;
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
	public override void SetCurrentHp(int hp)
	{
		CurrentHp = hp;
		if (CurrentHp <= 0 && !IsDead)
		{
			IsDead = true;
			_enemy.photonView.RPC(nameof(_enemy.RPC_EnemyDead), RpcTarget.AllBuffered, GetDeathExp());

			// 풀에서 삭제
			EnemySpawner.Instance.RemovePool(_enemy);
		}
	}
	public override void SetLevel(int level) => PokeLevel = level;
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
