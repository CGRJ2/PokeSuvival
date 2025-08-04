using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Panel_MatchMaking : MonoBehaviour
{
    [SerializeField] Transform roomInfoSlotsParent;
    [SerializeField] Button btn_LeftRoomListIndex;
    [SerializeField] Button btn_RightRoomListIndex;
    [SerializeField] TMP_Text tmp_RoomListIndex;
    [SerializeField] TMP_Text tmp_CurServer;

    [SerializeField] Button btn_ChangeServer;
    [SerializeField] Button btn_EnterRoom;
    [SerializeField] Button btn_CreateRoom;
    [SerializeField] Button btn_Esc;
    public Panel_LobbyServerList panel_LobbyServerList;

    int roomInfoListIndex = 0;
    int roomInfoListMaxIndex;

    Slot_RoomInfo[] roomInfoSlots;

    public void Init()
    {
        panel_LobbyServerList.Init();

        roomInfoSlots = roomInfoSlotsParent.GetComponentsInChildren<Slot_RoomInfo>();
        btn_ChangeServer.onClick.AddListener(() => UIManager.Instance.OpenPanel(panel_LobbyServerList.gameObject));

        btn_CreateRoom.onClick.AddListener(OpenCreateRoomPanel);
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void OnEnable()
    {
        tmp_CurServer.text = NetworkManager.Instance.CurServer.name;
    }

    public void SetDefaultSetting()
    {
        panel_LobbyServerList.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void UpdateRoomListView(List<RoomInfo> roomList)
    {
        // ǥ�� ������ �� ������ ��ü �� ������ ����� ��ü ������ ����
        if (roomList.Count % roomInfoSlots.Length == 0 && roomList.Count != 0)
            roomInfoListMaxIndex = roomList.Count / roomInfoSlots.Length - 1;
        else
            roomInfoListMaxIndex = roomList.Count / roomInfoSlots.Length;

        tmp_RoomListIndex.text = $"{roomInfoListIndex + 1} / {roomInfoListMaxIndex + 1}";

        for (int i = 0; i < roomInfoSlots.Length; i++)
        {
            // �ҷ��� �� ������ �� �̻� ���ٸ�
            if (roomList.Count - 1 < i)
            {
                roomInfoSlots[i].UpdateSlotView(null);
            }
            else
            {
                roomInfoSlots[i].UpdateSlotView(roomList[i + roomInfoListIndex * roomInfoSlots.Length]);
            }
        }
    }

    public void OpenCreateRoomPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.LobbyGroup.panel_RoomMaking.gameObject);
    }
}
