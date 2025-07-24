using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Panel_SelectStarting : MonoBehaviour
{
    public PokemonData selectedPokemon;
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
    }

    void SelectConfirm()
    {
        if (selectedPokemon == null) { Debug.LogError("������ ���ϸ��� �����ϴ�."); return; }

        // ��Ÿ�� ���ϸ� �������ֱ�
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["StartingPokemon"] = selectedPokemon;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        // ����׿�
        PokemonData debugTest = (PokemonData)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
        Debug.Log($"��Ÿ�� ���ϸ� ������: {debugTest.PokeName}");

        // LobbyDefault�� ��Ÿ�� ���ϸ� View ������Ʈ

        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

        // �г� �ݱ�
        CloseSelectPanel();
    }

    void CloseSelectPanel()
    {
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.panel_SelectStarting.gameObject);
    }
}
