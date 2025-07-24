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

        panel_MapSettings.Init();
        panel_RoomButtons.Init();
    }


    public void InitRoomView()
    {
        // 기본 방 설정(생성 시 설정된 데이터로 View 업데이트)
        tmp_RoomName.text = PhotonNetwork.CurrentRoom.Name;

        panel_MapSettings.InitRoomSettings(PhotonNetwork.IsMasterClient);
        panel_RoomButtons.InitButtons(PhotonNetwork.IsMasterClient);
        panel_MapSettings.UpdateRoomProperty();

        UIManager.Instance.OpenPanel(gameObject);
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
                else
                    roomMemberSlots[i].UpdateSlotView(null);
            }

            // 최대 인원보다 많은 슬롯이 있으면 슬롯 비활성화
            else
                roomMemberSlots[i].BlockSlot();
        }

        assignedSlots = instanceDic;
    }

    // 커스텀 프로퍼티 설정 직 후에 바로 불러오면, 딜레이에 의해 설정 전의 값이 불러와지므로. 해당 프로퍼티 값 설정될 때 까지 대기
    IEnumerator DelayedLoadCustomProperties(Player player, RoomMemberSlot roomMemberSlot)
    {
        yield return new WaitUntil(() => player.CustomProperties["Ready"] != null);
        Debug.Log(player.CustomProperties["Ready"]);
        Debug.Log($"{player.NickName}의 커스텀 프로퍼티:{player.CustomProperties["Ready"]}");

        bool ready = (bool)player.CustomProperties["Ready"];
        roomMemberSlot.UpdateReadyStateView(ready);
    }


    //// 룸 커스텀 프로퍼티 설정
    //ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtabl> ();
    //roomProperty["Map"] = "Select Map";
    //room.SetCustomProperties(roomProperty);

}


[System.Serializable]
public class MapData
{
    public string name;
    public Sprite sprite;
}