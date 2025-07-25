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
            Debug.LogError("��Ÿ�� ���ϸ��� �������ּ���");
            return;
        }

        // �ӽ�
        // �ΰ��� ���� �� ����ִ� ���� ã�� �����ؾ���.
        // �ΰ��� �������� �ο� ���¸� �����صδ� �߰��� �ʿ� => firebase DB ���� ��������
        nm.MoveToInGameScene("Test_InGameScene(CJM)");
    }

    public void OpenMatchMakingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_MatchMaking.gameObject);
    }
}
