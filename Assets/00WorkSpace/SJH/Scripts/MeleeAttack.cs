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
				// TODO : 대미지
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) return;
				BattleDataTable defenderData = new(iD.DefenderLevel, iD.DefenderPokeData, iD.DefenderPokeStat, iD.DefenderMaxHp, iD.DefenderCurrentHp);
				int damage = PokeUtils.CalculateDamage(attackerData, defenderData, skill);
				iD.TakeDamage(damage);
				Debug.Log($"{enemy.gameObject.name} 공격! 대미지 : {damage}");
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
				// TODO : 대미지
				var iD = enemy.GetComponent<IDamagable>();
				if (iD == null) return;
				BattleDataTable defenderData = new(iD.DefenderLevel, iD.DefenderPokeData, iD.DefenderPokeStat, iD.DefenderMaxHp, iD.DefenderCurrentHp);
				int damage = PokeUtils.CalculateDamage(attackerData, defenderData, skill);
				iD.TakeDamage(damage);
				Debug.Log($"{enemy.gameObject.name} 공격! 대미지 : {damage}");
			}
		}

		Debug.Log($"{skill.SkillName} 공격!");
	}
}
