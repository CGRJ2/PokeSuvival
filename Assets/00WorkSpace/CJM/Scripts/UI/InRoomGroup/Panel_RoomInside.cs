using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Panel_RoomInside : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_RoomName;
    RoomMemberSlot[] roomMemberSlots;
    [SerializeField] Transform roomMemberSlotsParent;
    //[SerializeField] TMP_Text tmp_PlayerCount;
    public Dictionary<Player, RoomMemberSlot> assignedSlots = new Dictionary<Player, RoomMemberSlot>();


    public Panel_MapSettings panel_MapSettings;
    public Panel_RoomButtons panel_RoomButtons;



    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdatePlayerList();
        }
    }


    public void Init()
    {
        roomMemberSlots = roomMemberSlotsParent.GetComponentsInChildren<RoomMemberSlot>();
        foreach (RoomMemberSlot slot in roomMemberSlots)
        {
            slot.Init();
        }

        panel_MapSettings.Init();
        panel_RoomButtons.Init();
    }


    public void InitRoomView()
    {
        // 기본 방 설정(생성 시 설정된 데이터로 View 업데이트)
        tmp_RoomName.text = PhotonNetwork.CurrentRoom.Name;

        panel_MapSettings.InitRoomSettings(PhotonNetwork.IsMasterClient);
        //panel_RoomButtons.InitButtons(PhotonNetwork.IsMasterClient);
        panel_MapSettings.UpdateRoomProperty();
    }

    public void UpdatePlayerList()
    {
        List<Player> playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        int maxPlayerCount = PhotonNetwork.CurrentRoom.MaxPlayers;
        //tmp_PlayerCount.text = $"{playerList.Count} / {maxPlayerCount}"; // 현재 방에 들어온 플레이어 수 표기

        Dictionary<Player, RoomMemberSlot> instanceDic = new Dictionary<Player, RoomMemberSlot>();


        for (int i = 0; i < roomMemberSlots.Length; i++)
        {
            if (i < maxPlayerCount)
            {
                if (i < playerList.Count)
                {
                    roomMemberSlots[i].UpdateSlotView(playerList[i]);
                    instanceDic.Add(playerList[i], roomMemberSlots[i]);
                    StartCoroutine(DelayedLoadCustomProperties(playerList[i], roomMemberSlots[i]));
                }
                // 현재 플레이어 수가 슬롯보다 적으면 빈슬롯으로 만들기
                else roomMemberSlots[i].UpdateSlotView(null);
            }

            // 최대 인원보다 많은 슬롯이 있으면 슬롯 비활성화
            else roomMemberSlots[i].BlockSlot();
        }

        // UI 갱신을 위한 assignedSlots 딕셔너리 할당
        assignedSlots = instanceDic;

        // 전원 Ready 상태라면 => MasterClient에 Start버튼 활성화
        if (!PhotonNetwork.IsMasterClient) { panel_RoomButtons.SetActiveStartButton(false); return; }

        foreach (Player player in playerList)
        {
            // 하나라도 Ready 상태가 아니면 비활성화
            if (!(player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"]))
            {
                panel_RoomButtons.SetActiveStartButton(false);
                return;
            }
            // 모두 Ready 상태면 활성화
            else panel_RoomButtons.SetActiveStartButton(true);
        }
    }

    // 방 입장 직 후에 바로 불러오면, 딜레이에 의해 설정 전의 값이 불러와지므로. 해당 프로퍼티 값 설정될 때 까지 대기
    IEnumerator DelayedLoadCustomProperties(Player player, RoomMemberSlot roomMemberSlot)
    {
        // 이런 WaitUntil로 무한 대기하는 구조들을 콜백 기반으로 리팩토링해주는 작업 필요 (TODO)


        yield return new WaitUntil(() => player.CustomProperties["Ready"] != null);

        bool ready = (bool)player.CustomProperties["Ready"];
        roomMemberSlot.UpdateReadyStateView(ready);
    }


}


[System.Serializable]
public class MapData
{
    public string name;
    public Sprite sprite;
}