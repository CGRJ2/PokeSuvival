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

        // �ӽ�
        // �ΰ��� ���� �� ����ִ� ���� ã�� �����ؾ���.
        // �ΰ��� �������� �ο� ���¸� �����صδ� �߰��� �ʿ� => firebase DB ���� ��������
        


        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "ebb39345-172c-4fe6-814b-f9a959a78382";
        PhotonNetwork.ConnectUsingSettings();

        if (PhotonNetwork.LocalPlayer.IsLocal)
            PhotonNetwork.LoadLevel("Test_InGameScene(CJM)");
    }
}
