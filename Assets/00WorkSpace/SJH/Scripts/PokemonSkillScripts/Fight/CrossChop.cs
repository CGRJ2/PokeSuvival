using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossChop : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		float size = attackerData.PokeData.PokeSize;
		float radius = size > 1 ? skill.Range + size : skill.Range;
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, radius);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.4f)
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) continue;

				iD.TakeDamage(attackerData, skill);
				PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", enemy.transform.position, Quaternion.identity);
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
