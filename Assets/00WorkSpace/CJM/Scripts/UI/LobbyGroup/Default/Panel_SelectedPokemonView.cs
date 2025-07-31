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
        // 스타팅 포켓몬이 설정되어 있다면 UI창 업데이트
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon") 
            && (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] != "None")
        {
            string pokemonDataSO_Name = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
            PokemonData selectedPokemonData = Define.GetPokeData(pokemonDataSO_Name);
                
            tmp_Name.text = selectedPokemonData.PokeName;
            image_Sprite.sprite = selectedPokemonData.PokemonInfoSprite;
        }
        // 설정이 안되어 있다면
        else
        {
            // 물음표 표시로 바꿔주기
            // 포켓몬 이름 text에 기본값 넣어주기
        }
    }

    void OpenPokemonListPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.StaticGroup.panel_SelectStarting.gameObject);
    }
}
