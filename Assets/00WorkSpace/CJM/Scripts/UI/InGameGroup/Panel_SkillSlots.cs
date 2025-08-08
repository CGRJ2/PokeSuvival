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
            // �ش� ���Կ� ���� ��ų�� �ִٸ�
            if (skillSlots[i].actived)
            {
                //Debug.Log($"{i+1}�� ��ų���� Ȱ��ȭ");
                skillSlots[i].UpdateSlotView(pokemonData.Skills[i]);
            }
            // �ش� ���Կ� ���� ��ų�� ���ٸ�
            else
            {
                //Debug.Log($"{i+1}�� ��ų���� ��Ȱ��ȭ");
                skillSlots[i].BlockSlotView();
            }
        }
    }


    public void UpdateSlotCoolTimeView(PlayerModel model, SkillSlot slot)
    {
        skillSlots[(int)slot].CoolTimeUpdate((model.SkillCooldownDic[slot]));
    }
}
