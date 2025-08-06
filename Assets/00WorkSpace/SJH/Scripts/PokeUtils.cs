using System.Linq;
using UnityEngine;

public static class PokeUtils
{
	// HP = [ { (종족값a x 2) + 100 } x 레벨 / 100 ] + 10(저레벨 보정)
	public static int CalculateHp(int level, int baseHp) => (int)(((baseHp * 2 + 100) * level / 100f) + 10);

	// 공방특공특방스피드 = [ { (종족값a x 2)} x 레벨 / 100 + 5(저레벨 보정)] x 성격보정
	public static int CalculateStat(int level, int baseStat) => (int)(((baseStat * 2) * level / 100f) + 5);

	public static int CalculateDamage(BattleDataTable attackerData, BattleDataTable defenderData, PokemonSkill skill)
	{
		#region 대미지 계산식
		// 대미지 계산식
		// 이 페이지에서 나오는 모든 공식은 왼쪽에서 오른쪽 순서대로 계산하며, 각 계산을 실행하기 전에 소수점 이하를 버린다.
		// (데미지 = (((((((레벨 × 2 ÷ 5) + 2) × 위력 × 특수공격 ÷ 50) ÷ 특수방어) × Mod1) + 2) × [[급소]] × Mod2 ×  랜덤수 ÷ 100) × 자속보정 × 타입상성1 × 타입상성2 × Mod3)
		// > (데미지 = (((((((레벨 × 2 ÷ 5) + 2) × 위력 × 특수공격 ÷ 50) ÷ 특수방어) × Mod1) + 2) × 랜덤수 ÷ 100) × 자속보정 × 타입상성1 × 타입상성2)

		// 필요 스탯
		// 레벨, 스킬 위력, 공 특공, 방 특방, 자속, 타입 상성1, 타입 상성2
		// Mod1 : 상태이상 ex) 화상
		// Mod2 : 생구, 메트로놈
		// Mod3 : 달인의띠,	위력 반감 열매
		#endregion

		int attackStat = 0;
		int defendStat = 0;
		switch (skill.SkillType)
		{
			case SkillType.Physical:
				attackStat = attackerData.AllStat.Attak;
				defendStat = defenderData.AllStat.Defense;
				break;
			case SkillType.Special:
				attackStat = attackerData.AllStat.SpecialAttack;
				defendStat = defenderData.AllStat.SpecialDefense;
				break;
		}

		float ran = Random.Range(0.85f, 1f);
		float sameTypeBonus = GetSameTypeBonus(skill.PokeType, attackerData.PokeData.PokeTypes);	// 자속 보정
		float typeBonus = GetTypeBonus(skill.PokeType, defenderData.PokeData.PokeTypes);            // 타입 보정

		int skillDamage = skill.Damage;

		// 지닌 도구 물공 특공 보정
		float itemBonus = 1f;
		if (attackerData.HeldItem != null)
		{
			var item = attackerData.HeldItem;
			// 힘의머리띠
			if (item.id == 10032 && skill.SkillType == SkillType.Physical)
			{
				itemBonus += item.value;
				Debug.Log($"{item.itemName} 보정! 기술 위력 {(int)(item.value * 100)}% 증가");
			}
			// 박식안경
			else if (item.id == 10028 && skill.SkillType == SkillType.Special)
			{
				itemBonus += item.value;
				Debug.Log($"{item.itemName} 보정! 기술 위력 {(int)(item.value * 100)}% 증가");
			}
			// 달인의 띠
			else if (item.id == 10027)
			{
				if (typeBonus >= 2f)
				{
					itemBonus += item.value;
					Debug.Log($"달인의 띠 보정! 효과가 굉장한 기술 위력 {(int)(item.value * 100)}% 증가");
				}
			}
			else if (item.BonusType == skill.PokeType)
			{
				itemBonus += item.value;
				Debug.Log($"{item.itemName} 보정! {item.BonusType} 타입 대미지 {(int)(item.value * 100)}% 증가");
			}
		}

		float step1 = Mathf.Floor((attackerData.Level * 2f) / 5f) + 2f;						// ((레벨 × 2 ÷ 5) + 2)
		float step2 = step1 * skillDamage * attackStat * itemBonus;    // ((레벨 × 2 ÷ 5) + 2) × 위력 × 특수공격
		float step3 = Mathf.Floor(step2 / 50f);												// (((레벨 × 2 ÷ 5) + 2) × 위력 × 특수공격 ÷ 50)
		float step4 = Mathf.Floor(step3 / defendStat);										// ((((레벨 × 2 ÷ 5) + 2) × 위력 × 특수공격 ÷ 50) ÷ 특수방어)

		float mod1 = 1f; // 상태이상 ex) 화상
		float totalDamage = step4 * mod1 * sameTypeBonus * typeBonus * ran;

		return typeBonus == 0 ? 0 : Mathf.Max((int)totalDamage, 1);
	}

	public static float GetTypeBonus(PokemonType attackType, params PokemonType[] defenders)
	{
		float result = 1f;
		foreach (var defType in defenders)
		{
			result *= Define.GetTypeDamageValue(attackType, defType);
		}
		return result;
	}

	public static float GetSameTypeBonus(PokemonType skillType, params PokemonType[] pokeTypes)
	{
		if (skillType == PokemonType.None) return 1f;
		foreach (var pokeType in pokeTypes)
		{
			if (pokeType == PokemonType.None) continue;
			if (pokeType == skillType) return 1.5f;
		}
		return 1f;
	}

	public static PokemonStat CalculateAllStat(int level, PokemonStat baseStat)
	{
		PokemonStat stat = new PokemonStat
		{
			Hp = CalculateHp(level, baseStat.Hp),
			Attak = CalculateStat(level, baseStat.Attak),
			Defense = CalculateStat(level, baseStat.Defense),
			SpecialAttack = CalculateStat(level, baseStat.SpecialAttack),
			SpecialDefense = CalculateStat(level, baseStat.SpecialDefense),
			Speed = CalculateStat(level, baseStat.Speed),
		};
		return stat;
	}

	public static int GetNextLevelExp(int currentLevel)
	{
		return (int)(5 * Mathf.Pow(currentLevel + 1, 3) / 4);
	}
}
