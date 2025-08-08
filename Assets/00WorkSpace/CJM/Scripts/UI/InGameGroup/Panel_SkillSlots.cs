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
                //Debug.Log($"{i+1}번 스킬슬롯 활성화");
                skillSlots[i].UpdateSlotView(pokemonData.Skills[i]);
            }
            // 해당 슬롯에 넣을 스킬이 없다면
            else
            {
                //Debug.Log($"{i+1}번 스킬슬롯 비활성화");
                skillSlots[i].BlockSlotView();
            }
        }
    }


    public void UpdateSlotCoolTimeView(PlayerModel model, SkillSlot slot)
    {
        skillSlots[(int)slot].CoolTimeUpdate((model.SkillCooldownDic[slot]));
    }
}
