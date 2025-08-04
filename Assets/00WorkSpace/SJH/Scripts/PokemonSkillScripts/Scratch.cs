using Photon.Pun;
using UnityEngine;

public class Scratch : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, skill.Range + attackerData.PokeData.PokeSize);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.7f) // 45
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) continue;
				iD.TakeDamage(attackerData, skill);
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
