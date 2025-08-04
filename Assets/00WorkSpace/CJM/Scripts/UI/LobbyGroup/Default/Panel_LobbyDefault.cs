using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Panel_LobbyDefault : MonoBehaviour
{
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_SelectMatch;
    [SerializeField] Button btn_MatchMaking;

    public Panel_SelectedPokemonView panel_PokemonView;
    public Panel_PlayerInfo panel_PlayerInfo;

    public void Init()
    {
        panel_PokemonView.Init();
        panel_PlayerInfo.Init();

        btn_QuickMatch.onClick.AddListener(QuickMatch);
        btn_SelectMatch.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_InGameServerList.gameObject));
        btn_MatchMaking.onClick.AddListener(OpenMatchMakingPanel);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void SetDefaultSetting()
    {
        
    }

    public void QuickMatch()
    {
        // ��Ÿ�� ���ϸ� Ŀ���� ������Ƽ�� �����Ѵٸ�
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
        {
            // ��Ÿ�� ���ϸ� Ŀ���� ������Ƽ�� ������ ������ ���¶��
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] == "None" ||
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
