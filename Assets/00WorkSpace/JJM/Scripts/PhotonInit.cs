using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // 마스터 서버에 연결
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버에 연결됨");
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new Photon.Realtime.RoomOptions { MaxPlayers = 10 }, Photon.Realtime.TypedLobby.Default);

    }

    public override void OnJoinedRoom()
    {
        Debug.Log("룸에 입장함 (PhotonInit)");
    }
}
