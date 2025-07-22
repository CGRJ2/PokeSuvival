using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] ServerData[] serverDatas;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
}


[System.Serializable]
public class ServerData
{
    public string name;
    public string id;
}
