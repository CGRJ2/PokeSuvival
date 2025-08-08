using UnityEngine;
using UnityEngine.UIElements;

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
			// 노말
			case "할퀴기": return new Scratch();
			case "몸통박치기": return new Tackle();
			// 풀
			case "잎날가르기": return new RazorLeaf();
			case "덩굴채찍": return new VineWhip();
			case "에너지볼": return new EnergyBall();
			// 불꽃
			case "불꽃세례": return new Ember();
			case "블라스트번": return new BlastBurn();
			case "도깨비불": return new WillOWisp();
			case "불대문자": return new FireBlast();
			case "쾌청": return new SunnyDay();
			default: return null;
		}
	}
}
