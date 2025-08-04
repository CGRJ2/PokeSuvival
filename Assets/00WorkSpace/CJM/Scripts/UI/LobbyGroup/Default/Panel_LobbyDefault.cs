using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

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
        // 스타팅 포켓몬 커스텀 프로퍼티가 존재한다면
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
        {
            // 스타팅 포켓몬 커스텀 프로퍼티의 내용이 누락된 상태라면
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] == "None" ||
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

    public void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
