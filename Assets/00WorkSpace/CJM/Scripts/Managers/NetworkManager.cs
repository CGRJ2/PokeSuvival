using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : SingletonPUN<NetworkManager>
{
    [Header("����� �뵵")]
    [SerializeField] TMP_Text tmp_State;


    [SerializeField] ServerData[] serverDatas;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    UIManager um;

    private void Awake() => Init();
    public void Init()
    {
        base.SingletonInit();
        um = UIManager.Instance;

        PhotonNetwork.ConnectUsingSettings();
        
        // ���� �õ��� ���ÿ� �ε�â���� ������
        if (um != null)
        {
            um.LoadingGroup.gameObject.SetActive(true);
            um.LoadingGroup.fullScreen.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // ���� ������
        if (tmp_State != null)
            tmp_State.text = $"Current State : {PhotonNetwork.NetworkClientState}";

        // �׽�Ʈ�� ���� ��ȯ
        if (Input.GetKeyDown(KeyCode.Y))
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "ebb39345-172c-4fe6-814b-f9a959a78382";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("����");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("������ ����");

        
        if (um != null)
        {
            // ����Ǹ� �ʱ�ȭ��(UI) Ȱ��ȭ
            um.InitializeGroup.InitView();

            // �ε�â ��Ȱ��ȭ
            um.LoadingGroup.fullScreen.gameObject.SetActive(false);
        }
            
    }

    // �κ� ����� ȣ���
    public override void OnJoinedLobby() 
    {
        if (um != null)
        {
            um.LobbyGroup.gameObject.SetActive(true);
            um.InitializeGroup.gameObject.SetActive(false);
            um.InGameGroup.gameObject.SetActive(false);
        }
    }


    #region ��ġ ����ŷ ����

    
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("�� �������");
    }

    // �� ����� ȣ���
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("�� ����");

        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.InitRoomView();
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
        }
    }

    // �� ����� ȣ���
    public override void OnLeftRoom() 
    {
        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(false);
        }
    }       

    // ���ο� �÷��̾ �� ����� ȣ���
    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList(); 
    }

    // �ٸ� �÷��̾ �� ����� ȣ���
    public override void OnPlayerLeftRoom(Player otherPlayer) 
    {
        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList(); 
    }


    // �κ� ���� �� ���� �߰� or ������ �� ������Ʈ ��
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log($"������ ���ŵ� �� ����{roomList.Count}");

        // ��ȭ�� �ֵ� ���� �� �ؽ����̺� ����
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                Debug.Log($"�� ���� {info.Name}");
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        Debug.Log($"���� �����ϴ� �� ����{cachedRoomList.Count}");

        // �ؽ����̺��� ����Ʈ�� ��ȯ
        List<RoomInfo> activedRoomList = cachedRoomList.Values.ToList();

        // Ȱ��ȭ�� ��(�������� ���� ��)�� ������Ʈ
        UIManager.Instance.LobbyGroup.panel_MatchMaking.UpdateRoomListView(activedRoomList);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (um != null)
            um.LobbyGroup.panel_RoomInside.panel_MapSettings.UpdateRoomProperty();
    }

    // ����� �÷��̾ Ŀ���� ������Ƽ�� ����� �� ȣ�� (�ٸ� ����� �����ص� ȣ���)
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    #endregion




    public void MoveToInGameScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }


    public void Test()
    {
        //PhotonNetwork.ConnectUsingSettings();   // ���� �õ� ��û
        //PhotonNetwork.Disconnect();             // ���� ���� ��û

        //PhotonNetwork.CreateRoom("RoomName");   // �� ���� ��û
        //PhotonNetwork.JoinRoom("RoomName");     // �� ���� ��û
        //PhotonNetwork.LeaveRoom();              // �� ���� ��û

        //PhotonNetwork.JoinLobby();              // �κ� ���� ��û
        //PhotonNetwork.LeaveLobby();             // �κ� ���� ��û

        //PhotonNetwork.LoadLevel("SceneName");   // �� ��ȯ ��û

        //bool isConnected = PhotonNetwork.IsConnected;           // ���� ���� Ȯ��
        //bool isInRoom = PhotonNetwork.InRoom;                   // �� ���� ���� Ȯ��
        //bool isLobby = PhotonNetwork.InLobby;                   // �κ� ���� ���� Ȯ��
        //ClientState state = PhotonNetwork.NetworkClientState;   // Ŭ���̾�Ʈ ���� Ȯ��
        //Player player = PhotonNetwork.LocalPlayer;              // ���� �÷��̾� ���� Ȯ��
        //Room players = PhotonNetwork.CurrentRoom;               // ���� �� ���� Ȯ��


        //public override void OnConnected() { }                          // ���� ���ӽ� ȣ���
        //public override void OnConnectedToMaster() { }                  // ������ ���� ���ӽ� ȣ���
        //public override void OnDisconnected(DisconnectCause cause) { }  // ���� ������ ȣ���

        //public override void OnCreatedRoom() { }    // �� ������ ȣ���
        //public override void OnJoinedRoom() { }     // �� ����� ȣ���
        //public override void OnLeftRoom() { }       // �� ����� ȣ���
        //public override void OnPlayerEnteredRoom(Player newPlayer) { }  // ���ο� �÷��̾ �� ����� ȣ���
        //public override void OnPlayerLeftRoom(Player otherPlayer) { }   // �ٸ� �÷��̾ �� ����� ȣ���
        //public override void OnCreateRoomFailed(short returnCode, string message) { }   // �� ���� ���н� ȣ���
        //public override void OnJoinRoomFailed(short returnCode, string message) { }     // �� ���� ���н� ȣ���

        //public override void OnJoinedLobby() { }    // �κ� ����� ȣ���
        //public override void OnLeftLobby() { }      // �κ� ����� ȣ���
        //public override void OnRoomListUpdate(List<RoomInfo> roomList) { }  // �� ��� ����� ȣ���

        //Room room = PhotonNetwork.CurrentRoom;  // ���� ������ ���� Ȯ��

        //// �� Ŀ���� ������Ƽ ����
        //ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtabl> ();
        //roomProperty["Map"] = "Select Map";
        //room.SetCustomProperties(roomProperty);

        //// �� Ŀ���� ������Ƽ Ȯ��
        //string curMap = (string)room.CustomProperties["Map"];

        //Player player = PhotonNetwork.LocalPlayer;  // �ڽ� �÷��̾ Ȯ��

        //// �÷��̾� Ŀ���� ������Ƽ ����
        //ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon> Hashtable();
        //playerProperty["Ready"] = true;
        //player.SetCustomProperties(playerProperty);

        //// �÷��̾� Ŀ���� ������Ƽ Ȯ��
        //bool ready = (bool)player.CustomProperties["Ready"];

    }

}

[System.Serializable]
public class ServerData
{
    public string name;
    public string id;
}