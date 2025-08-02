using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGroup_Lobby : MonoBehaviour
{
    public Panel_LobbyDefault panel_LobbyDefault;
    public Panel_MatchMaking panel_MatchMaking;
    public Panel_RoomInfo panel_RoomInfo;
    public Panel_RoomMaking panel_RoomMaking;
    public Panel_RoomInside panel_RoomInside;

    public void Init()
    {
        panel_LobbyDefault.Init();
        panel_MatchMaking.Init();
        panel_RoomInfo.Init();
        panel_RoomMaking.Init();
        panel_RoomInside.Init();
    }

    

    public void OnJoinedLobbyDefaultSetting()
    {
        panel_LobbyDefault.gameObject.SetActive(true);
        panel_MatchMaking.SetDefaultSetting();
        panel_RoomInfo.SetDefaultSetting();
        panel_RoomMaking.SetDefaultSetting();
        panel_RoomInside.SetDefaultSetting();
        UIManager.Instance.ClearPanelStack();
    }
}
