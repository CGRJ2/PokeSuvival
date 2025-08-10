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

    // 사망 경험치
    public int GetDeathExp()
	{
		int totalExp = 5 / 4 * PokeLevel * PokeLevel * PokeLevel;

		// 종족값
		int totalBaseStat = PokeData.BaseStat.GetBaseStat();

		// 가중치
		float modifyValue = totalBaseStat / 600f; // 600족기준

		return Mathf.RoundToInt(totalExp * modifyValue / 3);
	}
	public void SetHeal(int value)
	{
		if (IsDead || value <= 0) return;

		int newHp = Mathf.Min(_currentHp + value, MaxHp);
		CurrentHp = newHp;

		Debug.Log($"{value} 만큼 회복! 현재 체력 : {_currentHp}");
		_enemy.photonView.RPC(nameof(_enemy.RPC_SyncToCurrentHp), RpcTarget.AllBuffered, newHp);
	}
}
