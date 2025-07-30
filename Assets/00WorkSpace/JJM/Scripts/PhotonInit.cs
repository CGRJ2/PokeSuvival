using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // ������ ������ ����
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ������ �����");
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new Photon.Realtime.RoomOptions { MaxPlayers = 10 }, Photon.Realtime.TypedLobby.Default);

    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�뿡 ������ (PhotonInit)");
    }
}
