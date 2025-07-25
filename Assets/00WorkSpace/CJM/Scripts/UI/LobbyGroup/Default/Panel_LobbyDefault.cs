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
        btn_MatchMaking.onClick.AddListener(OpenMatchMakingPanel);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void QuickMatch()
    {
        NetworkManager nm = NetworkManager.Instance;

        if (PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] == null)
        {
            Debug.LogError("스타팅 포켓몬을 설정해주세요");
            return;
        }

        // 임시
        // 인게임 서버 중 비어있는 곳을 찾아 접속해야함.
        // 인게임 서버들의 인원 상태를 저장해두는 중계자 필요 => firebase DB 설계 진행하자
        nm.MoveToInGameScene("Test_InGameScene(CJM)");
    }

    public void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
