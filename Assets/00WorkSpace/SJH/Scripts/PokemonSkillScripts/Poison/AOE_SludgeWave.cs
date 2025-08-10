using System.Collections.Generic;
using UnityEngine;

public class AOE_SludgeWave : AOE_Skill
{
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
	}
}
