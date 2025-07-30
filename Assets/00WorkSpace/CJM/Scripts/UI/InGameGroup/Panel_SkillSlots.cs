using UnityEngine;

public class Panel_SkillSlots : MonoBehaviour
{
    SkillSlotView[] skillSlots;
    public void Init()
    {
        skillSlots = GetComponentsInChildren<SkillSlotView>();

    }


    public void UpdateSkillSlotsView(PlayerModel model)
    {
        PokemonData pokemonData = model.PokeData;

        foreach(SkillSlotView slotView in skillSlots)
        {
            slotView.actived = false;
        }

        for (int i = 0; i < pokemonData.Skills.Length; i++)
        {
            skillSlots[i].actived = true;
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            // 해당 슬롯에 넣을 스킬이 있다면
            if (skillSlots[i].actived)
            {
                skillSlots[i].UpdateSlotView(pokemonData.Skills[i]);
            }
            // 해당 슬롯에 넣을 스킬이 없다면
            else
            {
                skillSlots[i].BlockSlotView();
            }
        }


        float cool_01 = model.SkillCooldownDic[SkillSlot.Skill1];
    }
}
