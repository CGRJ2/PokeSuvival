public interface IDamagable
{
    BattleDataTable BattleData { get; }
    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill);
}
