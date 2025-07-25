using UnityEngine;

public class MeleeAttack : IAttack
{
	public MeleeAttack()
	{

	}

	public void Attack(Transform attacker, PokemonSkill skill)
	{
		switch (skill.SkillName)
		{
			case "할퀴기": FrontAttack(attacker, skill); break;
			case "몸통박치기": MoveAttack(attacker, skill); break;
		}
	}

	void FrontAttack(Transform attacker, PokemonSkill skill)
	{
		Debug.Log($"{skill.SkillName} 공격!");
	}

	void MoveAttack(Transform attacker, PokemonSkill skill)
	{
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
