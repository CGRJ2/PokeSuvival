
using UnityEngine;

public interface IAttack
{
    public void Attack(Transform attacker, Vector2 attackDir, PokemonSkill skill);
}
