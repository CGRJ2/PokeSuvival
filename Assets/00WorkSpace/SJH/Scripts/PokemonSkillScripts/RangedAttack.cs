using UnityEngine;

public class RangedAttack : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		switch (skill.SkillName)
		{
			case "잎날가르기": ShootProjectile(attacker, attackDir, attackerData, skill); break;
		}
	}

	public void ShootProjectile(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{

	}
}
