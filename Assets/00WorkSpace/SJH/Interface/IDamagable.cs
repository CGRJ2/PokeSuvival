using UnityEngine;

public interface IDamagable
{
    PokemonData DefenderPokeData { get; }
    PokemonStat DefenderPokeStat { get; }
    int DefenderLevel { get; }
    int DefenderMaxHp { get; }
    int DefenderCurrentHp { get; }
    public void TakeDamage(int value);
}
