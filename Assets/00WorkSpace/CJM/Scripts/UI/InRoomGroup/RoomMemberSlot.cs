using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomMemberSlot : MonoBehaviour, IPointerClickHandler
{
    //[SerializeField] TMP_Text playerLevel;
    [SerializeField] TMP_Text playerName;
    [SerializeField] Image image_Pokemon;
    [SerializeField] Sprite sprite_PokemonDefault;
    [SerializeField] Button btn_ChangePokemon;
    [SerializeField] GameObject readyPanel;
    [SerializeField] GameObject blockPanel;
   
    
    public void Init()
    {
        btn_ChangePokemon.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_SelectStarting.gameObject));
    }
   


    public void UpdateSlotView(Player player)
    {
        OpenSlot();

        // 빈 슬롯 View 업데이트
        if (player == null)
        {
            playerName.text = "";
            image_Pokemon.sprite = sprite_PokemonDefault;
            readyPanel.gameObject.SetActive(false);
            btn_ChangePokemon.gameObject.SetActive(false);
        }
        else
        {
            // 방장이라면 (Master 표시 추가)
            if (player.IsMasterClient)
            {
                playerName.text = $"{player.NickName} (Master)";
            }
            else
            {
                playerName.text = player.NickName;
            }

            // 포켓몬 데이터에서 이미지만 업데이트
            if (player.CustomProperties.ContainsKey("StartingPokemon"))
            {
                string pokemonDataSO_Name = (string)player.CustomProperties["StartingPokemon"];
                if (string.IsNullOrEmpty(pokemonDataSO_Name))
                {
                    image_Pokemon.sprite = sprite_PokemonDefault;
                }
                else
                {
                    PokemonData pokemonData = Define.GetPokeData(pokemonDataSO_Name);
                    image_Pokemon.sprite = pokemonData.PokemonInfoSprite;
                }
            }

            // 로컬 클라이언트에만 포켓몬 변경 버튼 활성화
            if (player == PhotonNetwork.LocalPlayer)
            {
                if (!btn_ChangePokemon.gameObject.activeSelf)
                    btn_ChangePokemon.gameObject.SetActive(true);
            }
            else
            {
                if (btn_ChangePokemon.gameObject.activeSelf)
                    btn_ChangePokemon.gameObject.SetActive(false);
            }
        }
    }

    public void OpenSlot()
    {
        blockPanel.SetActive(false);
    }

    public void BlockSlot()
    {
        blockPanel.SetActive(true);
    }

    public void UpdateReadyStateView(bool isReady)
    {
        readyPanel.SetActive(isReady);
    }


    //IPointerClickHandler - OnPointerClick
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭 상호작용
        if (eventData.button == PointerEventData.InputButton.Left) 
        {

        }
        // 우클릭 상호작용
        else if (eventData.button == PointerEventData.InputButton.Right)
        {

        }
    }
}
