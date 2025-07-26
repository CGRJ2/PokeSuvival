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
        PokemonData selectedPokemonData = (PokemonData)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];

        tmp_Name.text = selectedPokemonData.PokeName;
        image_Sprite.sprite = selectedPokemonData.PokemonIconSprite; // <= 여기 스탠딩 스프라이트로 바꿔줘야함
    }

    void OpenPokemonListPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_SelectStarting.gameObject);
    }
}
