using UnityEngine;

public class SkillStrategyAttack : IAttack
{
	private IAttack _attack;
	public SkillStrategyAttack(string skillName)
	{
		_attack = CreateSkill(skillName);
	}

	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		if (_attack == null) return;
		_attack.Attack(attacker, attackDir, attackerData, skill);
	}

	IAttack CreateSkill(string skillName)
	{
		switch (skillName)
		{
			case "할퀴기": return new Scratch();
			case "몸통박치기": return new Tackle();
			case "잎날가르기": return new RazorLeaf();
			default: return null;
		}
	}
}
