using UnityEngine;

public class AOE_Explosion : AOE_Skill
{
	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);

		Collider2D[] enemies = Physics2D.OverlapCircleAll(attacker.transform.position, skill.Range);
		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			var iD = enemy.GetComponent<IDamagable>();
			if (iD == null) continue;
			iD.TakeDamage(attackerData, skill);
		}

		if (attackerData.PC != null)
		{
			attackerData.PC.Model.SetCurrentHp(-1);
		}
		else
		{
			var enemy = _attacker.GetComponent<Enemy>();
			enemy?.EnemyData.SetCurrentHp(-1);
		}
	}
}
