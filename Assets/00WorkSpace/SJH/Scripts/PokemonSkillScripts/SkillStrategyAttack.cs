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
			case "불꽃세례": return new Ember();
			case "블라스트번": return new BlastBurn();
			case "도깨비불": return new WillOWisp();
			case "불대문자": return new FireBlast();
			case "쾌청": return new SunnyDay();
			default: return null;
		}
	}
}
