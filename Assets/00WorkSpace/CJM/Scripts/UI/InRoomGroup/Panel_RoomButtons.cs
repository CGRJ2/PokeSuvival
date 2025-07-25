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
        // ���� or Not ����
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

        // �̰� �׳� ��� �ο��� Ready ������ ��, ���常 Start ��ư�� Ȱ��ȭ �ǵ��� ����.
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.panel_RoomInside.gameObject);
    }

    public void Ready()
    {
        //// �� Ŀ���� ������Ƽ ����
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            RoomMemberSlot localPlayerSlot = UIManager.Instance.LobbyGroup.panel_RoomInside.assignedSlots[PhotonNetwork.LocalPlayer];
            // �̹� ���� ���¶�� ���� false
            if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Ready"])
            {
                btn_Ready.GetComponent<Image>().color = Color.white;

                ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
                playerProperty["Ready"] = false;
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

                localPlayerSlot.UpdateReadyStateView(false);
            }
            // ���� ���°� �ƴ϶�� ���� true
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
