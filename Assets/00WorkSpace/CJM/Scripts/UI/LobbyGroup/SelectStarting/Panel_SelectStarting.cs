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

        // 맨 처음 보여줄 몬스터 (일단 1번인 이상해씨를 넣었습니다)
        panel_PokemonInfo.UpdateView(Define.GetPokeData("이상해씨"));
        selectedPokemon = Define.GetPokeData("이상해씨");
    }

    void SelectConfirm()
    {
        if (selectedPokemon == null) { Debug.LogError("선택한 포켓몬이 없습니다."); return; }

        // 스타팅 포켓몬 설정해주기 (포톤 로컬 유저 커스텀 프로퍼티)
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["StartingPokemon"] = selectedPokemon.PokeName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        // 로그인을 한 유저라면
        if (BackendManager.Auth.CurrentUser != null)
        {
            // UserData에 업데이트
            BackendManager.Instance.UpdateUserData("startingPokemonName", selectedPokemon.PokeName);
        }

        // LobbyDefault에 스타팅 포켓몬 View 업데이트
        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

        // 패널 닫기
        CloseSelectPanel();
    }


    void CloseSelectPanel()
    {
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.StaticGroup.panel_SelectStarting.gameObject);
    }
}
