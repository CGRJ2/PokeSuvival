using Photon.Pun;
using UnityEngine;

public class FuryCutter : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + (attackDir * skill.Range);
		//Quaternion rot = Quaternion.FromToRotation(Vector2.right, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);

		float size = attackerData.PokeData.PokeSize;
		float radius = size > 1 ? skill.Range + size : skill.Range;
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, radius);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.3f)
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) continue;
				iD.TakeDamage(attackerData, skill);
				PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.name}Effect", enemy.transform.position, Quaternion.identity);
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
