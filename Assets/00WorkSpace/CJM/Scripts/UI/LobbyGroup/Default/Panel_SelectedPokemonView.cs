using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비에서 현재 선택되어있는 포켓몬을 보여주는 용도. (1. 이름 / 2. 스프라이트 이 둘 만 표기)
public class Panel_SelectedPokemonView : MonoBehaviour
{

    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] Image image_Sprite;
    [SerializeField] Button btn_changePokemon;
    [SerializeField] Sprite sprite_noneSettedImage;


    public void Init()
    {
        btn_changePokemon.onClick.AddListener(OpenPokemonListPanel);
    }

    public void UpdateView()
    {
        Debug.Log("포켓몬 인포 패널 업데이트");

        // 로그인을 한 유저라면
        if (BackendManager.Auth.CurrentUser != null)
        {
            BackendManager.Instance.LoadUserDataFromDB((userData) =>
            {
                // 스타팅 포켓몬이 설정되어 있다면 UI창 업데이트
                if (userData.startingPokemonName != "None")
                {
                    string pokemonDataSO_Name = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
                    PokemonData selectedPokemonData = Define.GetPokeData(pokemonDataSO_Name);

                    tmp_Name.text = selectedPokemonData.PokeName;
                    image_Sprite.sprite = selectedPokemonData.PokemonInfoSprite;
                }
                // 설정이 안되어 있다면
                else
                {
                    // 포켓몬 이름 text에 기본값 넣어주기
                    tmp_Name.text = "스타팅 포켓몬을\r\n설정해주세요";

                    // 물음표 표시로 바꿔주기
                    image_Sprite.sprite = sprite_noneSettedImage;
                }
            });
        }

        // 게스트 로그인을 한 유저라면
        else
        {
            // 포톤 로컬 유저 커스텀 프로퍼티로 스타팅 포켓몬 설정 여부 판단
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon"))
            {
                // 스타팅 포켓몬이 설정되어 있다면 UI창 업데이트
                if (!string.IsNullOrEmpty((string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"]))
                {
                    string pokemonDataSO_Name = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
                    PokemonData selectedPokemonData = Define.GetPokeData(pokemonDataSO_Name);

                    tmp_Name.text = selectedPokemonData.PokeName;
                    image_Sprite.sprite = selectedPokemonData.PokemonInfoSprite;
                }
                // 스타팅 포켓몬 프로퍼티는 있는데 설정이 안되어 있다면
                else
                {
                    // 포켓몬 이름 text에 기본값 넣어주기
                    tmp_Name.text = "스타팅 포켓몬을\r\n설정해주세요";

                    // 물음표 표시로 바꿔주기
                    image_Sprite.sprite = sprite_noneSettedImage;
                }
            }
            // 스타팅 포켓몬 프로퍼티 자체가 없다면
            else
            {
                // 포켓몬 이름 text에 기본값 넣어주기
                tmp_Name.text = "스타팅 포켓몬을\r\n설정해주세요";

                // 물음표 표시로 바꿔주기
                image_Sprite.sprite = sprite_noneSettedImage;
            }
        }
    }

    void OpenPokemonListPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.StaticGroup.panel_SelectStarting.gameObject);
    }
}
