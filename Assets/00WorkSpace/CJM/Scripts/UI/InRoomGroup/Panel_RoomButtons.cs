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
        //// 룸 커스텀 프로퍼티 설정
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            // 스타팅 포켓몬을 정하지 않은 상태라면 레디 못함
            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("StartingPokemon")) 
            { 
                Debug.LogError("스타팅 포켓몬을 설정하지 않아 READY를 할 수 없습니다."); 
                return; 
            }

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

    public void SetActiveStartButton(bool activate)
    {
        btn_Start.gameObject.SetActive(activate);
    }

    public void StartWithParty()
    {
        // 임시
        string selectedMapKey = (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
        int memberCount = PhotonNetwork.CurrentRoom.PlayerCount;

        BackendManager.Instance.GetServerData(selectedMapKey, ServerType.InGame, (targetServer) =>
        {
            BackendManager.Instance.IsAbleToConnectMultipleUserIntoServer(targetServer, memberCount, (accessable) =>
            {
                if (accessable)
                {
                    // 서버에 자리 예약
                    BackendManager.Instance.OnEnterServerCapacityUpdate(targetServer, memberCount, () =>
                    {
                        // 자리 예약 성공 시 => 룸 커스텀 프로퍼티에 시작가능 갱신
                        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
                        roomProperty["Start"] = true;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
                    });
                }
                else
                {
                    Debug.LogError("파티에 모든 멤버가 이동하기에 서버에 자리가 부족합니다.");
                }
            });
        });
    }
}
