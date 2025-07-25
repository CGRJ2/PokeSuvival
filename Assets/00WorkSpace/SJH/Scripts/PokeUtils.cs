
public static class PokeUtils
{
	// HP = [ { (종족값a x 2) + 100 } x 레벨 / 100 ] + 10(저레벨 보정)
	public static int CalculateHp(int level, int baseHp) => (int)(((baseHp * 2 + 100) * level / 100f) + 10);

	// 공방특공특방스피드 = [ { (종족값a x 2)} x 레벨 / 100 + 5(저레벨 보정)] x 성격보정
	public static int CalculateStat(int level, int baseStat) => (int)(((baseStat * 2) * level / 100f) + 5);


	public static int CalculateDamage()
	{
		// TODO : 대미지 계산식
		// 공격 타입
		// 위력
		// 공격자 스탯
		// 방어자 스탯
		return 1;
	}
}
