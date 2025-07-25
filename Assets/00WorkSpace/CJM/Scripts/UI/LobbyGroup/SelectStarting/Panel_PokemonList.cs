using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Panel_PokemonList : MonoBehaviour
{
    [SerializeField] Transform slotListParent;
    Slot_StartingPokemon[] slots;
    
    
    // ���� �߰��ϸ� ���뺰 SO ������ �� �ֵ��� UI �߰� ����


    public void Init()
    {
        slots = slotListParent.GetComponentsInChildren<Slot_StartingPokemon>();
        UpdateAllSlotView();
    }

    void UpdateAllSlotView()
    {
        PokemonData[] allPokemonDatas = Resources.LoadAll<PokemonData>("PokemonSO");

        // ���ϸ� ������ ���� ����Ʈ �г��� ���� �������� ���ٸ� �����
        if (allPokemonDatas.Length > slots.Length) 
            Debug.LogError($"���ϸ� ������ SO�� ���Ժ��� ����! [SO ����:{allPokemonDatas.Length}] [���� ����: {slots.Length}]... ���� {allPokemonDatas.Length- slots.Length}�� �߰� �ʿ�");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < allPokemonDatas.Length)
            {
                slots[i].UpdateSlotView(allPokemonDatas[i]);
            }
            // ��ü ���� �������� ����� ���ϸ�SO������ �� ������ => ���� ��Ȱ��ȭ
            else
            {
                slots[i].UpdateSlotView(null);
            }
        }
    }


    
}
