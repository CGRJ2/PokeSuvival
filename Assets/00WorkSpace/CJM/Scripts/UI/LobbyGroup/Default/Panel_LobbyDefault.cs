using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Panel_LobbyDefault : MonoBehaviour
{
    
    [Header("왼쪽 패널 버튼들")]
    [SerializeField] Button btn_Ranking;
    [SerializeField] Button btn_Shop;
    [SerializeField] Button btn_Option;


    [Header("매치 메이킹 관련 버튼")]
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_SelectMatch;
    [SerializeField] Button btn_MatchMaking;

    [Header("패널")]
    public Panel_SelectedPokemonView panel_PokemonView;
    public Panel_PlayerInfo panel_PlayerInfo;
    public Panel_PlayerRecords panel_PlayerRecords;

    public void Init()
    {
        panel_PokemonView.Init();
        panel_PlayerInfo.Init();
        panel_PlayerRecords.Init();

        btn_Ranking.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_RankingBoard.gameObject));

        btn_QuickMatch.onClick.AddListener(QuickMatch);
        btn_SelectMatch.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_InGameServerList.gameObject));
        btn_MatchMaking.onClick.AddListener(OpenMatchMakingPanel);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void SetDefaultSetting()
    {
        if (panel_PlayerRecords.isOpened)
            panel_PlayerRecords.SwitchToggleDropDownButton();
    }

    public void QuickMatch()
    {
        // 스타팅 포켓몬 커스텀 프로퍼티가 존재한다면
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
        {
            // 스타팅 포켓몬 커스텀 프로퍼티의 내용이 누락된 상태라면
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] == "" ||
                string.IsNullOrEmpty((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"]))
            {
                Debug.LogError("스타팅 포켓몬 커스텀 프로퍼티는 존재하지만, 내용물이 누락됨!");
            }
            // 잘 설정되어있으면 시작
            else
            {
                NetworkManager.Instance.MoveToInGameScene();
            }
        }
        // 커스텀 프로퍼티 설정이 안되어 있다면 인게임 시작 못함
        else
        {
            Debug.LogError("스타팅 포켓몬을 설정해주세요");
            return;
        }


    }

    private void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
