using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomMemberSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text playerName;
    //[SerializeField] TMP_Text playerLevel;
    [SerializeField] GameObject readyPanel;
    [SerializeField] GameObject blockPanel;
    

    public void UpdateSlotView(Player player)
    {
        OpenSlot();
        // 빈 슬롯 View 업데이트
        if (player == null)
        {
            playerName.text = "";
            readyPanel.gameObject.SetActive(false);
        }
        else
        {
            // 방장이라면
            if (player.IsMasterClient)
            {
                playerName.text = $"{player.NickName} (Master)";
            }
            else
            {
                playerName.text = player.NickName;
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
