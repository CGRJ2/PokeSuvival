using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// �κ񿡼� ���� ���õǾ��ִ� ���ϸ��� �����ִ� �뵵. (1. �̸� / 2. ��������Ʈ �� �� �� ǥ��)
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
        // ��Ÿ�� ���ϸ� �������ֱ�
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
