using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Panel_SelectStarting : MonoBehaviour
{
    [HideInInspector] public PokemonData selectedPokemon;
    public Panel_PokemonList panel_PokemonList;
    public Panel_PokemonInfo panel_PokemonInfo;
    [SerializeField] Button btn_Confirm;
    [SerializeField] Button btn_Cancel;
    public void Init()
    {
        panel_PokemonList.Init();
        panel_PokemonInfo.Init();
        btn_Confirm.onClick.AddListener(SelectConfirm);
        btn_Cancel.onClick.AddListener(CloseSelectPanel);

        // �� ó�� ������ ���� (�ϴ� 1���� �̻��ؾ��� �־����ϴ�)
        panel_PokemonInfo.UpdateView(Define.GetPokeData("�̻��ؾ�"));
        selectedPokemon = Define.GetPokeData("�̻��ؾ�");
    }

    void SelectConfirm()
    {
        if (selectedPokemon == null) { Debug.LogError("������ ���ϸ��� �����ϴ�."); return; }

        // ��Ÿ�� ���ϸ� �������ֱ� (���� ���� ���� Ŀ���� ������Ƽ)
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["StartingPokemon"] = selectedPokemon.PokeName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        // �α����� �� �������
        if (BackendManager.Auth.CurrentUser != null)
        {
            // UserData�� ������Ʈ
            BackendManager.Instance.UpdateUserData("startingPokemonName", selectedPokemon.PokeName);
        }

        // LobbyDefault�� ��Ÿ�� ���ϸ� View ������Ʈ
        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

        // �г� �ݱ�
        CloseSelectPanel();
    }


    void CloseSelectPanel()
    {
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.StaticGroup.panel_SelectStarting.gameObject);
    }
}
