using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot_StartingPokemon : MonoBehaviour
{
    PokemonData pokemonData;
    [SerializeField] Image image;
    [SerializeField] Button btn_Self;

    private void Start()
    {
        btn_Self.onClick.AddListener(UpdateSelectedPokemonInfoView);
    }

    public void UpdateSlotView(PokemonData pokemonData)
    {
        this.pokemonData = pokemonData;
        
        if (pokemonData != null)
        {
            image.sprite = pokemonData.PokemonIconSprite;
            
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
        else
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

    public void UpdateSelectedPokemonInfoView()
    {
        UIGroup_Lobby lobbyGroup = UIManager.Instance.LobbyGroup;

        lobbyGroup.panel_SelectStarting.panel_PokemonInfo.UpdateView(pokemonData);
        lobbyGroup.panel_SelectStarting.selectedPokemon = this.pokemonData;
    }
}
