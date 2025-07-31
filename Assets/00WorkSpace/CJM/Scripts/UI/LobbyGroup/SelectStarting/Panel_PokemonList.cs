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
        PokemonData[] allStartingPokemons = Resources.LoadAll<PokemonData>("PokemonSO/StartingPokemons/");

        // ���ϸ� ������ ���� ����Ʈ �г��� ���� �������� ���ٸ� �����
        if (allStartingPokemons.Length > slots.Length) 
            Debug.LogError($"���ϸ� ������ SO�� ���Ժ��� ����! [SO ����:{allStartingPokemons.Length}] [���� ����: {slots.Length}]... ���� {allStartingPokemons.Length- slots.Length}�� �߰� �ʿ�");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < allStartingPokemons.Length)
            {
                slots[i].UpdateSlotView(allStartingPokemons[i]);
            }
            // ��ü ���� �������� ����� ���ϸ�SO������ �� ������ => ���� ��Ȱ��ȭ
            else
            {
                slots[i].UpdateSlotView(null);
            }
        }
    }


    
}
