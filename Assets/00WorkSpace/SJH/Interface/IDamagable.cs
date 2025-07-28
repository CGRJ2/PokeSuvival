public interface IDamagable
{
    BattleDataTable BattleData { get; }
    public void TakeDamage(int value);
}
