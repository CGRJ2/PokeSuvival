using Photon.Pun;
using UnityEngine;

public class VineWhip : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Quaternion rot = Quaternion.FromToRotation(Vector2.right, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", Vector3.zero, rot);
		go.transform.SetParent(attacker.transform, false);

		float size = attackerData.PokeData.PokeSize;
		float radius = size > 1 ? skill.Range + size : skill.Range;
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, radius);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.1f) // 45
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) continue;
				iD.TakeDamage(attackerData, skill);
				PhotonNetwork.Instantiate($"PokemonSkillPrefabs/VineWhipEffect", enemy.transform.position, Quaternion.identity);
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
