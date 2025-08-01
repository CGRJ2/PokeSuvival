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
            // ��Ÿ�� ���ϸ��� �����Ǿ� �ִٸ� �ΰ��� �� ���̵�
            if (userData.startingPokemonName != "None")
            {
               NetworkManager.Instance.MoveToInGameScene();
            }
            // ������ �ȵǾ� �ִٸ� �ΰ��� ���� ����
            else
            {
                Debug.LogError("��Ÿ�� ���ϸ��� �������ּ���");
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
