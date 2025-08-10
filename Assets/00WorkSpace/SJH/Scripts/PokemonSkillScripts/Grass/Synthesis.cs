using Photon.Pun;
using UnityEngine;

public class Synthesis : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(1.3f);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(1.3f);

		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", attacker.transform.position, Quaternion.identity);

		// 플레이어
		if (attackerData.PC != null)
		{
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
