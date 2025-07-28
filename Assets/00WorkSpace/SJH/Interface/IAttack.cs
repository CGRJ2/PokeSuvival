
using UnityEngine;

public interface IAttack
{
    public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill);
}
