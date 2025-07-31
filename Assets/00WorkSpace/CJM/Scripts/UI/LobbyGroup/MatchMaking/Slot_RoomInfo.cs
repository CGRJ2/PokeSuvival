using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_RoomInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_RoomIndex;
    [SerializeField] TMP_Text tmp_RoomName;
    [SerializeField] TMP_Text tmp_PlayerCount;
    Button btn_Self;
    RoomInfo roomInfo;
    public int roomIndex;

    private void Awake()
    {
        btn_Self = GetComponent<Button>();
        btn_Self.onClick.AddListener(OpenRoomInfoPanel);
    }

    private void OpenRoomInfoPanel()
    {
        UIManager um = UIManager.Instance;

        Panel_RoomInfo panel_RoomInfo = um.LobbyGroup.panel_RoomInfo;

        panel_RoomInfo.UpdatePanelView(roomInfo);
        um.OpenPanel(panel_RoomInfo.gameObject);
    }

    public void UpdateSlotView(RoomInfo roomInfo)
    {
        if (roomInfo == null)
        {
            this.roomInfo = null;

            tmp_RoomIndex.text = "";
            tmp_RoomName.text = "";
            tmp_PlayerCount.text = "";
            gameObject.SetActive(false);
        }
        else
        {
            this.roomInfo = roomInfo;

            tmp_RoomIndex.text = roomIndex.ToString();
            tmp_RoomName.text = roomInfo.Name;
            tmp_PlayerCount.text = $"{roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
            gameObject.SetActive(true);
        }
    }
}
