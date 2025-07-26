using UnityEngine;

public class MeleeAttack : IAttack
{
	public MeleeAttack()
	{

	}

	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		switch (skill.SkillName)
		{
			case "할퀴기": FrontAttack(attacker, attackDir, attackerData, skill); break;
			case "몸통박치기": MoveAttack(attacker, attackDir, attackerData, skill); break;
		}
	}

	void FrontAttack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, skill.Range);
		if (enemies.Length <= 0) return;
		
		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.707f) // 45
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) return;
				int damage = PokeUtils.CalculateDamage(attackerData, iD.BattleData, skill);
				iD.TakeDamage(damage);
				Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{iD.BattleData.Level} {iD.BattleData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}

	void MoveAttack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, skill.Range);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.707f) // 45
			{
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) return;
				int damage = PokeUtils.CalculateDamage(attackerData, iD.BattleData, skill);
				iD.TakeDamage(damage);
				Debug.Log($"{attackerData.PokeData.PokeName} 이/가 {iD.BattleData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
