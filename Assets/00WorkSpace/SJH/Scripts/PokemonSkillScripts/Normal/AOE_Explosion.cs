using UnityEngine;

public class AOE_Explosion : AOE_Skill
{
	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);
	}

	public void Attack()
	{
		Collider2D[] enemies = Physics2D.OverlapCircleAll(_attacker.transform.position, _skill.Range);
		foreach (var enemy in enemies)
		{
			if (_attacker == enemy.transform) continue;

			var iD = enemy.GetComponent<IDamagable>();
			if (iD == null) continue;
			iD.TakeDamage(_attackerData, _skill);
		}

		if (_attackerData.PC != null)
		{
			_attackerData.PC.Model.SetCurrentHp(-1);
		}
		else
		{
			var enemy = _attacker.GetComponent<Enemy>();
			enemy?.EnemyData.SetCurrentHp(-1);
		}
	}
}
