using System.Collections.Generic;
using System.Linq;
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

		if (skill.SkillName == "손가락흔들기")
		{
			// 손가락흔들기면 랜덤 스킬을 사용
			// 스킬 리스트 중 손가락흔들기를 제외한 스킬을 랜덤 반환
			var init = Define.GetPokeSkillData("손가락흔들기");
			List<string> skillNameList = Define.PokeSkillDic.Values
				.Where(skill => skill.SkillName != "손가락흔들기" /*&& skill.SkillName != "대폭발*/)
				.Select(skill => skill.SkillName)
				.ToList();
			string ranSkillName = skillNameList[Random.Range(0, skillNameList.Count)];
			var ranAttack = CreateSkill(ranSkillName);

			// 손가락흔들기
			_attack.Attack(attacker, attackDir, attackerData, skill);
			Debug.Log("손가락흔들기 사용");
			// 랜덤스킬
			var ranSkill = Define.GetPokeSkillData(ranSkillName);
			ranAttack.Attack(attacker, attackDir, attackerData, ranSkill);
			Debug.Log($"{ranSkill.SkillName} 사용");
		}
		else
		{
			_attack.Attack(attacker, attackDir, attackerData, skill);
		}
	}

	IAttack CreateSkill(string skillName)
	{
		switch (skillName)
		{
			// 노말
			case "할퀴기": return new Scratch();
			case "몸통박치기": return new Tackle();
			case "트라이어택": return new TriAttack();
			case "연속뺨치기": return new DoubleSlap();
			case "파괴광선": return new HyperBeam();
			case "대폭발": return new Explosion();
			case "손가락흔들기": return new Metronome();

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
			case "브레이브버드": return new BraveBird();

			// 전기
			case "전기쇼크": return new Thundershock();
			case "방전": return new Discharge();
			case "번개": return new Thunder();

			// 땅
			case "진흙뿌리기": return new MudSlap();
			case "모래지옥": return new SandTomb();

			// 드래곤
			case "용의숨결": return new DragonBreath();
			case "드래곤클로": return new DragonClaw();

			// 바위
			case "돌떨구기": return new RockThrow();
			case "록블라스트": return new RockBlast();

			// 고스트
			case "섀도볼": return new ShadowBall();
			case "섀도클로": return new ShadowClaw();

			// 에스퍼
			case "사이코키네시스": return new Psychic();
			case "환상빔": return new Psybeam();
			case "최면술": return new Hypnosis();

			// 독
			case "오물폭탄": return new SludgeBomb();
			case "오물웨이브": return new SludgeWave();

			// 격투
			case "진공파": return new VacuumWave();
			case "크로스촙": return new CrossChop();
			case "폭발펀치": return new DynamicPunch();

			// 악
			case "깨물어부수기": return new Crunch();

			// 얼음
			case "얼음뭉치": return new IceShard();
			case "얼음엄니": return new IceFang();

			// 페어리
			case "치근거리기": return new PlayRough();

			// 강철
			case "불릿펀치": return new BulletPunch();
			case "아이언헤드": return new IronHead();


			default: return null;
		}
	}
}
