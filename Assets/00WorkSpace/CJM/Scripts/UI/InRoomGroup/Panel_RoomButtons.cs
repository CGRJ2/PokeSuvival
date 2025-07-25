using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_RoomButtons : MonoBehaviour
{
    [SerializeField] Button btn_Start;
    [SerializeField] Button btn_Ready;
    [SerializeField] Button btn_Exit;
    public void Init()
    {
        btn_Exit.onClick.AddListener(ExitRoom);
        btn_Ready.onClick.AddListener(Ready);
    }

    public void InitButtons(bool isMaster)
    {
        // 방장 or Not 설정
        if (isMaster)
        {
            btn_Ready.interactable = false;
            btn_Ready.gameObject.SetActive(false);

            btn_Start.interactable = true;
            btn_Start.gameObject.SetActive(true);
        }
        else
        {
            btn_Ready.interactable = true;
            btn_Ready.gameObject.SetActive(true);

            btn_Start.interactable = false;
            btn_Start.gameObject.SetActive(false);
        }

        // 이거 그냥 모든 인원이 Ready 상태일 때, 방장만 Start 버튼이 활성화 되도록 하자.
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.panel_RoomInside.gameObject);
    }

    public void Ready()
    {
        //// 룸 커스텀 프로퍼티 설정
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            RoomMemberSlot localPlayerSlot = UIManager.Instance.LobbyGroup.panel_RoomInside.assignedSlots[PhotonNetwork.LocalPlayer];
            // 이미 레디 상태라면 레디 false
            if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Ready"])
            {
                btn_Ready.GetComponent<Image>().color = Color.white;

                ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
                playerProperty["Ready"] = false;
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

                localPlayerSlot.UpdateReadyStateView(false);
            }
            // 레디 상태가 아니라면 레디 true
            else
            {
                btn_Ready.GetComponent<Image>().color = Color.green;

                ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
                playerProperty["Ready"] = true;
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

                localPlayerSlot.UpdateReadyStateView(true);
            }
        }
    }
}
