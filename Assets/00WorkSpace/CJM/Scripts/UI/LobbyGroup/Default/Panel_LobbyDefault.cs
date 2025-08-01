using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyDefault : MonoBehaviour
{
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_MatchMaking;
    
    public Panel_SelectedPokemonView panel_PokemonView;
    public Panel_PlayerInfo panel_PlayerInfo;

    public void Init()
    {
        panel_PokemonView.Init();
        panel_PlayerInfo.Init();

        btn_QuickMatch.onClick.AddListener(QuickMatch);
        btn_MatchMaking.onClick.AddListener(OpenMatchMakingPanel);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void QuickMatch()
    {
        BackendManager.Instance.LoadUserDataFromDB((userData) =>
        {
            // 스타팅 포켓몬이 설정되어 있다면 인게임 씬 퀵이동
            if (userData.startingPokemonName != "None")
            {
               NetworkManager.Instance.MoveToInGameScene();
            }
            // 설정이 안되어 있다면 인게임 시작 못함
            else
            {
                Debug.LogError("스타팅 포켓몬을 설정해주세요");
                return;
            }
        });
    }

    public void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
