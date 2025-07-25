using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비에서 현재 선택되어있는 포켓몬을 보여주는 용도. (1. 이름 / 2. 스프라이트 이 둘 만 표기)
public class Panel_SelectedPokemonView : MonoBehaviour
{
    
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] Image image_Sprite;
    [SerializeField] Button btn_changePokemon;


    public void Init()
    {
        btn_changePokemon.onClick.AddListener(OpenPokemonListPanel);
    }

    public void UpdateView()
    {
        // 스타팅 포켓몬 설정해주기
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
        {
            string pokemonDataSO_Name = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
            PokemonData selectedPokemonData = Resources.Load<PokemonData>($"PokemonSO/{pokemonDataSO_Name}");

            tmp_Name.text = selectedPokemonData.PokeName;
            image_Sprite.sprite = selectedPokemonData.PokemonInfoSprite;
        }
    }

    void OpenPokemonListPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_SelectStarting.gameObject);
    }
}
