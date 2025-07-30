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
    [Header("테스트용 인게임 씬 이름")] // 나중에 DB로 관리할 예정
    public string temp_InGameSceneName;

    [Header("테스터 서버 연결 여부")]
    [SerializeField] bool isTestServer;
    [SerializeField] int testServerIndex;

    [Header("디버깅 용도")]
    [SerializeField] TMP_Text tmp_State;
    ServerData curServer;
    [SerializeField] ServerData[] testServerDatas;
    [SerializeField] ServerData[] lobbyServerDatas;
    [SerializeField] ServerData[] inGameServerDatas;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    UIManager um;

    Hashtable customPropsDB_Player; // 서버 이동 시, 커스텀 프로퍼티를 유지하기 위한 백업 공간

    private void Awake() => Init();
    public void Init()
    {
        base.SingletonInit();
        um = UIManager.Instance;

        // 서버 관리 R&D 진행 후 수정하자
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

        // 연결 시도와 동시에 로딩창으로 가리기
        if (um != null)
        {
            um.LoadingGroup.gameObject.SetActive(true);
            um.LoadingGroup.fullScreen.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // 상태 디버깅용
        if (tmp_State != null)
            tmp_State.text = $"현재 서버 : {curServer.name}, Current State : {PhotonNetwork.NetworkClientState}";
    }


    public override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("연결");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("마스터 연결");

        // 현재 접속한 서버 판단이 필요해

        // 현재 접속한 서버가 로비라면
        if (curServer.type == ServerType.Lobby)
        {
            // 플레이어 정보가 없으면(= 처음 시작한 상태라면) => InitializeGroup(UI) 활성화
            // 이거를 지금은 닉네임으로 판단하지만, firebase를 적용하고부터는 PlayerData의 유무로 판단하자
            if (PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty())
                um.InitializeGroup.InitView();
            else
                StartCoroutine(JoinLobbyAfterConnectedMaster());
        }
        // 현재 접속한 서버가 인게임 서버라면
        else if (curServer.type == ServerType.InGame)
        {
            // 서버 이동 전에 설정된 값 동기화. Room 설정
            // 파티로 입장한 경우 & 퀵매치로 입장한 경우 나누어서 설정
            //PhotonNetwork.JoinRoom("서버 이동 전에 선택한 맵 key");
        }
        else if (curServer.type == ServerType.TestServer)
        {
            StartCoroutine(JoinLobbyAfterConnectedMaster());
            return;
        }

        // 로딩창 비활성화
        if (um.LoadingGroup != null)
            um.LoadingGroup.fullScreen.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator JoinLobbyAfterConnectedMaster()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }


    // 로비 입장시 호출됨
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

            // 플레이어 정보 업데이트
            if (BackendManager.Auth.CurrentUser != null)
                um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdateView();
            else
            {
                // 게스트로그인이면 플레이어 정보 패널 위치에 로그인 버튼 활성화
            }
        }
    }


    #region 매치 메이킹 관련


    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("방 만들어짐");
    }

    // 방 입장시 호출됨
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("방 입장");

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

    // 방 퇴장시 호출됨
    public override void OnLeftRoom()
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(false);
        }
    }

    // 새로운 플레이어가 방 입장시 호출됨
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    // 다른 플레이어가 방 퇴장시 호출됨
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }


    // 로비에 있을 때 방을 추가 or 삭제할 때 업데이트 됨
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (curServer.type == ServerType.TestServer) { return; }
        if (curServer.type == ServerType.InGame) { return; }


        base.OnRoomListUpdate(roomList);
        //Debug.Log($"정보가 갱신된 방 개수{roomList.Count}");

        // 변화한 애들 현재 방 해시테이블에 갱신
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                //Debug.Log($"방 삭제 {info.Name}");
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        //Debug.Log($"현재 존재하는 방 개수{cachedRoomList.Count}");

        // 해시테이블을 리스트로 변환
        List<RoomInfo> activedRoomList = cachedRoomList.Values.ToList();

        // 활성화된 방(삭제되지 않은 방)만 업데이트
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

    // 방안의 플레이어가 커스텀 프로퍼티가 변경될 때 호출 (다른 사람이 변경해도 호출됨)
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
        // 현재 접속 중인 서버가 없다면
        if (curServer == null) { Debug.LogError("현재 서버를 인식하지 못함!"); }

        // 플레이어 커스텀 프로퍼티 백업
        if (PhotonNetwork.LocalPlayer.CustomProperties != null)
            customPropsDB_Player = PhotonNetwork.LocalPlayer.CustomProperties;

        // 현재 접속 중인 서버 연결 해제
        PhotonNetwork.Disconnect();
        // 이동할 서버 AppID 갱신
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;
        // 갱신된 ID의 서버 연결
        PhotonNetwork.ConnectUsingSettings();

        // 연결 후 커스텀 프로퍼티 복원
        PhotonNetwork.LocalPlayer.CustomProperties = customPropsDB_Player;

        // 접속에 성공하면 현재 서버 갱신
        curServer = serverData;
    }

    public void MoveToLobby()
    {
        // 우선 첫번째 서버로 고정 이동
        ChangeServer(lobbyServerDatas[0]);

        PhotonNetwork.LoadLevel("LobbyScene(CJM)"); // 로비 씬 이름 씬매니저에 저장해두기
    }

    public void MoveToInGameScene(string sceneName)
    {
        // 우선 첫번째 서버로 고정 이동
        ChangeServer(inGameServerDatas[0]);

        PhotonNetwork.LoadLevel(sceneName);




    }


    public void Test()
    {
        //PhotonNetwork.ConnectUsingSettings();   // 접속 시도 요청
        //PhotonNetwork.Disconnect();             // 접속 해제 요청

        //PhotonNetwork.CreateRoom("RoomName");   // 방 생성 요청
        //PhotonNetwork.JoinRoom("RoomName");     // 방 입장 요청
        //PhotonNetwork.LeaveRoom();              // 방 퇴장 요청

        //PhotonNetwork.JoinLobby();              // 로비 입장 요청
        //PhotonNetwork.LeaveLobby();             // 로비 퇴장 요청

        //PhotonNetwork.LoadLevel("SceneName");   // 씬 전환 요청

        //bool isConnected = PhotonNetwork.IsConnected;           // 접속 여부 확인
        //bool isInRoom = PhotonNetwork.InRoom;                   // 방 입장 여부 확인
        //bool isLobby = PhotonNetwork.InLobby;                   // 로비 입장 여부 확인
        //ClientState state = PhotonNetwork.NetworkClientState;   // 클라이언트 상태 확인
        //Player player = PhotonNetwork.LocalPlayer;              // 포톤 플레이어 정보 확인
        //Room players = PhotonNetwork.CurrentRoom;               // 현재 방 정보 확인


        //public override void OnConnected() { }                          // 포톤 접속시 호출됨
        //public override void OnConnectedToMaster() { }                  // 마스터 서버 접속시 호출됨
        //public override void OnDisconnected(DisconnectCause cause) { }  // 접속 해제시 호출됨

        //public override void OnCreatedRoom() { }    // 방 생성시 호출됨
        //public override void OnJoinedRoom() { }     // 방 입장시 호출됨
        //public override void OnLeftRoom() { }       // 방 퇴장시 호출됨
        //public override void OnPlayerEnteredRoom(Player newPlayer) { }  // 새로운 플레이어가 방 입장시 호출됨
        //public override void OnPlayerLeftRoom(Player otherPlayer) { }   // 다른 플레이어가 방 퇴장시 호출됨
        //public override void OnCreateRoomFailed(short returnCode, string message) { }   // 방 생성 실패시 호출됨
        //public override void OnJoinRoomFailed(short returnCode, string message) { }     // 방 입장 실패시 호출됨

        //public override void OnJoinedLobby() { }    // 로비 입장시 호출됨
        //public override void OnLeftLobby() { }      // 로비 퇴장시 호출됨
        //public override void OnRoomListUpdate(List<RoomInfo> roomList) { }  // 방 목록 변경시 호출됨

        //Room room = PhotonNetwork.CurrentRoom;  // 현재 참가한 룸을 확인

        //// 룸 커스텀 프로퍼티 설정
        //ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtabl> ();
        //roomProperty["Map"] = "Select Map";
        //room.SetCustomProperties(roomProperty);

        //// 룸 커스텀 프로퍼티 확인
        //string curMap = (string)room.CustomProperties["Map"];

        //Player player = PhotonNetwork.LocalPlayer;  // 자신 플레이어를 확인

        //// 플레이어 커스텀 프로퍼티 설정
        //ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon> Hashtable();
        //playerProperty["Ready"] = true;
        //player.SetCustomProperties(playerProperty);

        //// 플레이어 커스텀 프로퍼티 확인
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

