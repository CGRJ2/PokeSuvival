using Photon.Pun;
using UnityEngine;

public class Synthesis : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", Vector3.zero, Quaternion.identity);
		go.transform.SetParent(attacker, false);

		// 플레이어
		if (attackerData.PC != null)
		{
			var pc = attackerData.PC;
			pc.Model.SetHeal(pc.Model.MaxHp / 2);
		}
		// 몬스터
		else
		{
			var enemy = attacker.GetComponent<Enemy>();
			enemy.EnemyData.SetHeal(enemy.EnemyData.MaxHp / 2);
		}
	}
}
