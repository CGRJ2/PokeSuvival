using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Panel_PokemonList : MonoBehaviour
{
    [SerializeField] Transform slotListParent;
    Slot_StartingPokemon[] slots;
    
    
    // 세대 추가하면 세대별 SO 갱신할 수 있도록 UI 추가 적용


    public void Init()
    {
        slots = slotListParent.GetComponentsInChildren<Slot_StartingPokemon>();
        UpdateAllSlotView();
    }

    void UpdateAllSlotView()
    {
        PokemonData[] allStartingPokemons = Resources.LoadAll<PokemonData>("PokemonSO/StartingPokemons/");

        // 포켓몬 데이터 수가 리스트 패널의 슬롯 개수보다 많다면 디버깅
        if (allStartingPokemons.Length > slots.Length) 
            Debug.LogError($"포켓몬 데이터 SO가 슬롯보다 많음! [SO 개수:{allStartingPokemons.Length}] [슬롯 개수: {slots.Length}]... 슬롯 {allStartingPokemons.Length- slots.Length}개 추가 필요");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < allStartingPokemons.Length)
            {
                slots[i].UpdateSlotView(allStartingPokemons[i]);
            }
            // 전체 슬롯 개수보다 저장된 포켓몬SO개수가 더 적으면 => 슬롯 비활성화
            else
            {
                slots[i].UpdateSlotView(null);
            }
        }
    }


    
}
