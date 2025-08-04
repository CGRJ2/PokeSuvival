using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
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
        btn_Start.onClick.AddListener(StartWithParty);
    }
    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void Ready()
    {
        //// �� Ŀ���� ������Ƽ ����
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            // ��Ÿ�� ���ϸ��� ������ ���� ���¶�� ���� ����
            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon")) 
            { 
                Debug.LogError("��Ÿ�� ���ϸ��� �������� �ʾ� READY�� �� �� �����ϴ�."); 
                return; 
            }

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

    public void SetActiveStartButton(bool activate)
    {
        btn_Start.gameObject.SetActive(activate);
    }

    public void StartWithParty()
    {
        // �ӽ�
        string selectedMapKey = (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
        int memberCount = PhotonNetwork.CurrentRoom.PlayerCount;

        BackendManager.Instance.GetServerData(selectedMapKey, ServerType.InGame, (targetServer) =>
        {
            BackendManager.Instance.IsAbleToConnectMultipleUserIntoServer(targetServer, memberCount, (accessable) =>
            {
                if (accessable)
                {
                    // ������ �ڸ� ����
                    BackendManager.Instance.OnEnterServerCapacityUpdate(targetServer, memberCount, () =>
                    {
                        // �ڸ� ���� ���� �� => �� Ŀ���� ������Ƽ�� ���۰��� ����
                        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
                        roomProperty["Start"] = true;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
                    });
                }
                else
                {
                    Debug.LogError("��Ƽ�� ��� ����� �̵��ϱ⿡ ������ �ڸ��� �����մϴ�.");
                }
            });
        });
    }
}
