using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyDefault : MonoBehaviour
{
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_MatchMaking;

    public Panel_SelectedPokemonView panel_PokemonView;

    public void Init()
    {
        panel_PokemonView.Init();

        btn_QuickMatch.onClick.AddListener(QuickMatch);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void QuickMatch()
    {
        NetworkManager nm = NetworkManager.Instance;

        // 임시
        // 인게임 서버 중 비어있는 곳을 찾아 접속해야함.
        // 인게임 서버들의 인원 상태를 저장해두는 중계자 필요 => firebase DB 설계 진행하자
        


        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "ebb39345-172c-4fe6-814b-f9a959a78382";
        PhotonNetwork.ConnectUsingSettings();

        if (PhotonNetwork.LocalPlayer.IsLocal)
            PhotonNetwork.LoadLevel("Test_InGameScene(CJM)");
    }
}
