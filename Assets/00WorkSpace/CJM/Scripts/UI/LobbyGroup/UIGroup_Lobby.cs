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

    public void Init()
    {
        panel_LobbyDefault.Init();
        panel_MatchMaking.Init();
        panel_RoomInfo.Init();
        panel_RoomMaking.Init();
    }
}
