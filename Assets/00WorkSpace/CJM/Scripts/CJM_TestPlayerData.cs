using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CJM_TestPlayerData : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // TODO
            string pokemonDataSO_Name = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
            PokemonData pokemonData = Define.GetPokeData("pokemonDataSO_Name");
            Debug.Log($"�ΰ��� �÷��̾��� ���ϸ�: {pokemonData.PokeName}");
        }
    }
}
