using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : SingletonPUN<NetworkManager>
{
    [Header("�׽�Ʈ�� �ΰ��� �� �̸�")] // ���߿� DB�� ������ ����
    public string temp_InGameSceneName;

    [Header("�׽��� ���� ���� ����")]
    [SerializeField] bool isTestServer;
    [SerializeField] int testServerIndex;

    [Header("����� �뵵")]
    [SerializeField] TMP_Text tmp_State;
    ServerData curServer;
    [SerializeField] ServerData[] testServerDatas;
    [SerializeField] ServerData[] lobbyServerDatas;
    [SerializeField] ServerData[] inGameServerDatas;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    UIManager um;

    Hashtable customPropsDB_Player; // ���� �̵� ��, Ŀ���� ������Ƽ�� �����ϱ� ���� ��� ����

    private void Awake() => Init();
    public void Init()
    {
        base.SingletonInit();
        um = UIManager.Instance;

        // ���� ���� R&D ���� �� ��������
        curServer = lobbyServerDatas[0];
        if (isTestServer)
        {
            ChangeServer(testServerDatas[testServerIndex]);
        }
        else
        {
            ChangeServer(lobbyServerDatas[0]);
        }
        //PhotonNetwork.ConnectUsingSettings();

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
            tmp_State.text = $"���� ���� : {curServer.name}, Current State : {PhotonNetwork.NetworkClientState}";
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

        // ���� ������ ���� �Ǵ��� �ʿ���

        // ���� ������ ������ �κ���
        if (curServer.type == ServerType.Lobby)
        {
            // �÷��̾� ������ ������(= ó�� ������ ���¶��) => InitializeGroup(UI) Ȱ��ȭ
            // �̰Ÿ� ������ �г������� �Ǵ�������, firebase�� �����ϰ���ʹ� PlayerData�� ������ �Ǵ�����
            if (PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty())
                um.InitializeGroup.InitView();
            else
                StartCoroutine(JoinLobbyAfterConnectedMaster());
        }
        // ���� ������ ������ �ΰ��� �������
        else if (curServer.type == ServerType.InGame)
        {
            // ���� �̵� ���� ������ �� ����ȭ. Room ����
            // ��Ƽ�� ������ ��� & ����ġ�� ������ ��� ����� ����
            //PhotonNetwork.JoinRoom("���� �̵� ���� ������ �� key");
        }
        else if (curServer.type == ServerType.TestServer)
        {
            StartCoroutine(JoinLobbyAfterConnectedMaster());
            return;
        }

        // �ε�â ��Ȱ��ȭ
        if (um.LoadingGroup != null)
            um.LoadingGroup.fullScreen.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator JoinLobbyAfterConnectedMaster()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }


    // �κ� ����� ȣ���
    public override void OnJoinedLobby()
    {
        if (curServer.type == ServerType.TestServer)
        {
            return;
        }

        if (um != null)
        {
            um.LobbyGroup.gameObject.SetActive(true);
            um.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

            um.InitializeGroup.gameObject.SetActive(false);
            um.InGameGroup.gameObject.SetActive(false);

            // �÷��̾� ���� ������Ʈ
            if (BackendManager.Auth.CurrentUser != null)
                um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdateView();
            else
            {
                // �Խ�Ʈ�α����̸� �÷��̾� ���� �г� ��ġ�� �α��� ��ư Ȱ��ȭ
            }
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

        if (curServer.type == ServerType.TestServer) { return; }

        if (curServer.type == ServerType.InGame)
        {
            if (um != null)
                um.LobbyGroup.gameObject.SetActive(false);
        }
        else if (curServer.type == ServerType.Lobby)
        {
            if (um != null)
            {
                um.LobbyGroup.panel_RoomInside.InitRoomView();
                um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
                um.LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
            }
        }
    }

    // �� ����� ȣ���
    public override void OnLeftRoom()
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(false);
        }
    }

    // ���ο� �÷��̾ �� ����� ȣ���
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    // �ٸ� �÷��̾ �� ����� ȣ���
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }


    // �κ� ���� �� ���� �߰� or ������ �� ������Ʈ ��
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        base.OnRoomListUpdate(roomList);
        //Debug.Log($"������ ���ŵ� �� ����{roomList.Count}");

        // ��ȭ�� �ֵ� ���� �� �ؽ����̺� ����
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                //Debug.Log($"�� ���� {info.Name}");
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        //Debug.Log($"���� �����ϴ� �� ����{cachedRoomList.Count}");

        // �ؽ����̺��� ����Ʈ�� ��ȯ
        List<RoomInfo> activedRoomList = cachedRoomList.Values.ToList();

        // Ȱ��ȭ�� ��(�������� ���� ��)�� ������Ʈ
        UIManager.Instance.LobbyGroup.panel_MatchMaking.UpdateRoomListView(activedRoomList);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.panel_MapSettings.UpdateRoomProperty();
    }

    // ����� �÷��̾ Ŀ���� ������Ƽ�� ����� �� ȣ�� (�ٸ� ����� �����ص� ȣ���)
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }

        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    #endregion


    public void ChangeServer(ServerData serverData)
    {
        // ���� ���� ���� ������ ���ٸ�
        if (curServer == null) { Debug.LogError("���� ������ �ν����� ����!"); }

        // �÷��̾� Ŀ���� ������Ƽ ���
        if (PhotonNetwork.LocalPlayer.CustomProperties != null)
            customPropsDB_Player = PhotonNetwork.LocalPlayer.CustomProperties;

        // ���� ���� ���� ���� ���� ����
        PhotonNetwork.Disconnect();
        // �̵��� ���� AppID ����
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;
        // ���ŵ� ID�� ���� ����
        PhotonNetwork.ConnectUsingSettings();

        // ���� �� Ŀ���� ������Ƽ ����
        PhotonNetwork.LocalPlayer.CustomProperties = customPropsDB_Player;

        // ���ӿ� �����ϸ� ���� ���� ����
        curServer = serverData;
    }

    public void MoveToLobby()
    {
        // �켱 ù��° ������ ���� �̵�
        ChangeServer(lobbyServerDatas[0]);

        PhotonNetwork.LoadLevel("LobbyScene(CJM)"); // �κ� �� �̸� ���Ŵ����� �����صα�
    }

    public void MoveToInGameScene(string sceneName)
    {
        // �켱 ù��° ������ ���� �̵�
        ChangeServer(inGameServerDatas[0]);

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
    public ServerType type;
    public string name;
    public string id;
}
public enum ServerType { Lobby, InGame, TestServer };

