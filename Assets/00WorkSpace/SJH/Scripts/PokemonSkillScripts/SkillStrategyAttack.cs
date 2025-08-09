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
			case "트라이어택": return new TriAttack();
			// 풀
			case "잎날가르기": return new RazorLeaf();
			case "덩굴채찍": return new VineWhip();
			case "에너지볼": return new EnergyBall();
			case "광합성": return new Synthesis();
			// 불꽃
			case "불꽃세례": return new Ember();
			case "블라스트번": return new BlastBurn();
			case "도깨비불": return new WillOWisp();
			case "불대문자": return new FireBlast();
			case "쾌청": return new SunnyDay();
			// 물
			case "거품광선": return new BubbleBeam();
			case "아쿠아커터": return new AquaCutter();
			case "하이드로펌프": return new HydorPump();
			// 비행
			case "에어커터": return new AirCutter();
			case "폭풍": return new Hurricane();


            // 전기
            case "전기쇼크": return new Thundershock();

            // 땅
            case "진흙뿌리기": return new MudSlap();

            // 바위
            case "돌떨구기": return new RockThrow();
            case "록블라스트": return new RockBlast();


            // 드래곤
            case "용의숨결": return new DragonBreath();
            // 고스트
            case "섀도볼": return new ShadowBall();
            // 에스퍼
            case "사이코키네시스": return new Psychic();

            // 독
            case "오물폭탄": return new SludgeBomb();

            default: return null;
		}
	}
}
