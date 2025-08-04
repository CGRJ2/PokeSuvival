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
        // �⺻ �� ����(���� �� ������ �����ͷ� View ������Ʈ)
        tmp_RoomName.text = PhotonNetwork.CurrentRoom.Name;

        panel_MapSettings.InitRoomSettings(PhotonNetwork.IsMasterClient);
        //panel_RoomButtons.InitButtons(PhotonNetwork.IsMasterClient);
        panel_MapSettings.UpdateRoomProperty();
    }

    public void UpdatePlayerList()
    {
        List<Player> playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        int maxPlayerCount = PhotonNetwork.CurrentRoom.MaxPlayers;
        //tmp_PlayerCount.text = $"{playerList.Count} / {maxPlayerCount}"; // ���� �濡 ���� �÷��̾� �� ǥ��

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
                // ���� �÷��̾� ���� ���Ժ��� ������ �󽽷����� �����
                else roomMemberSlots[i].UpdateSlotView(null);
            }

            // �ִ� �ο����� ���� ������ ������ ���� ��Ȱ��ȭ
            else roomMemberSlots[i].BlockSlot();
        }

        // UI ������ ���� assignedSlots ��ųʸ� �Ҵ�
        assignedSlots = instanceDic;

        // ���� Ready ���¶�� => MasterClient�� Start��ư Ȱ��ȭ
        if (!PhotonNetwork.IsMasterClient) { panel_RoomButtons.SetActiveStartButton(false); return; }

        foreach (Player player in playerList)
        {
            // �ϳ��� Ready ���°� �ƴϸ� ��Ȱ��ȭ
            if (!(player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"]))
            {
                panel_RoomButtons.SetActiveStartButton(false);
                return;
            }
            // ��� Ready ���¸� Ȱ��ȭ
            else panel_RoomButtons.SetActiveStartButton(true);
        }
    }

    // �� ���� �� �Ŀ� �ٷ� �ҷ�����, �����̿� ���� ���� ���� ���� �ҷ������Ƿ�. �ش� ������Ƽ �� ������ �� ���� ���
    IEnumerator DelayedLoadCustomProperties(Player player, RoomMemberSlot roomMemberSlot)
    {
        // �̷� WaitUntil�� ���� ����ϴ� �������� �ݹ� ������� �����丵���ִ� �۾� �ʿ� (TODO)


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