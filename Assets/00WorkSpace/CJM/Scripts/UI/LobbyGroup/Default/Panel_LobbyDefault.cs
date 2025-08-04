using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Panel_LobbyDefault : MonoBehaviour
{
    
    [Header("���� �г� ��ư��")]
    [SerializeField] Button btn_Ranking;
    [SerializeField] Button btn_Shop;
    [SerializeField] Button btn_Option;


    [Header("��ġ ����ŷ ���� ��ư")]
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_SelectMatch;
    [SerializeField] Button btn_MatchMaking;

    [Header("�г�")]
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
        // ��Ÿ�� ���ϸ� Ŀ���� ������Ƽ�� �����Ѵٸ�
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
        {
            // ��Ÿ�� ���ϸ� Ŀ���� ������Ƽ�� ������ ������ ���¶��
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] == "" ||
                string.IsNullOrEmpty((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"]))
            {
                Debug.LogError("��Ÿ�� ���ϸ� Ŀ���� ������Ƽ�� ����������, ���빰�� ������!");
            }
            // �� �����Ǿ������� ����
            else
            {
                NetworkManager.Instance.MoveToInGameScene();
            }
        }
        // Ŀ���� ������Ƽ ������ �ȵǾ� �ִٸ� �ΰ��� ���� ����
        else
        {
            Debug.LogError("��Ÿ�� ���ϸ��� �������ּ���");
            return;
        }


    }

    private void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
