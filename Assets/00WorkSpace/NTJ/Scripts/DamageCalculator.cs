using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageCalculator
{
    public static float GetDamageMultiplier(
        PokemonType attackType,
        PokemonType defenderType,
        List<ItemPassive> equippedPassives)
    {
        float baseMultiplier = Define.GetTypeDamageValue(attackType, defenderType);

        foreach (var passive in equippedPassives)
        {
            if (passive == null || passive.elementalBoosts == null) continue;

            foreach (var boost in passive.elementalBoosts)
            {
                if (boost.type == attackType)
                {
                    baseMultiplier *= boost.multiplier;
                    Debug.Log($"[속성 강화] {attackType} x{boost.multiplier} → {baseMultiplier}");
                }
            }
        }

        return baseMultiplier;
    }
}
