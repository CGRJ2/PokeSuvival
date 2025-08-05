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
            if (passive != null && passive.elementalBoosts.Contains(attackType))
            {
                baseMultiplier *= 1.2f; // �Ӽ� ��ȭ ���� ����
            }
        }

        return baseMultiplier;
    }
}
