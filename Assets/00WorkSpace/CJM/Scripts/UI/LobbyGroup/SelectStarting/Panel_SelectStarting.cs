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
        if (selectedPokemon == null) { Debug.LogError("선택한 포켓몬이 없습니다."); return; }

        // 스타팅 포켓몬 설정해주기
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["StartingPokemon"] = selectedPokemon;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        // 디버그용
        PokemonData debugTest = (PokemonData)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
        Debug.Log($"스타팅 포켓몬 설정됨: {debugTest.PokeName}");

        // LobbyDefault에 스타팅 포켓몬 View 업데이트

        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

        // 패널 닫기
        CloseSelectPanel();
    }

    void CloseSelectPanel()
    {
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.panel_SelectStarting.gameObject);
    }
}
