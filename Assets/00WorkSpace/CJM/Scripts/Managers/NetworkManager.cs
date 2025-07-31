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
    [Header("디버깅 용도")]
    [SerializeField] TMP_Text tmp_State;

    [Header("테스터 서버 연결 여부")]
    [SerializeField] bool isTestServer;
    [SerializeField] int testServerIndex;

    [Header("씬 전환을 위한 이름(string) 저장")]
    public string inGameSceneName;
    public string lobbySceneName;
    
    public ServerData CurServer { get; private set; }

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    UIManager um;

    public void Init()
    {
        base.SingletonInit();
        um = UIManager.Instance;

        Debug.Log("초기화 진행. 로비 서버로 연결 시작");
        StartCoroutine(InitLobbyServerAfterBackendInitComplete());


        // 연결 시도와 동시에 로딩창으로 가리기
        if (um != null)
        {
            um.StaticGroup.panel_Loading.gameObject.SetActive(true);
        }
    }
    System.Collections.IEnumerator InitLobbyServerAfterBackendInitComplete()
    {
        yield return new WaitUntil(() => BackendManager.Auth != null);
        yield return new WaitUntil(() => BackendManager.Database != null);

        // 로비 서버로 연결
        ConnectToBestServer(ServerType.Lobby);
    }


    private void Update()
    {
        // 상태 디버깅용
        if (tmp_State != null)
        {
            if (CurServer != null)
                tmp_State.text = $"현재 서버 : {CurServer.name}, Current State : {PhotonNetwork.NetworkClientState}";
            else
                tmp_State.text = "현재 접속된 서버 없음";
        }
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

        // 현재 접속한 서버가 로비라면
        if (CurServer.type == (int)ServerType.Lobby)
        {
            // 플레이어 정보가 없으면(= 처음 시작한 상태라면) => InitializeGroup(UI) 활성화
            if (PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty())
            {
                um.InitializeGroup.InitView();
            }
            else
            {
                StartCoroutine(JoinLobbyAfterConnectedMaster());
            }
        }
        // 현재 접속한 서버가 인게임 서버라면
        else if (CurServer.type == (int)ServerType.InGame)
        {
            StartCoroutine(JoinLobbyAfterConnectedMaster());
        }

        // 테스트용 서버 예외처리 
        else if (CurServer.type == (int)ServerType.FunctionTestServer)
        {
            StartCoroutine(JoinLobbyAfterConnectedMaster());
        }
        else if (CurServer.type == (int)ServerType.TestServer)
        {
            Debug.Log("연결됐으니 초기화 화면 활성화");
            um.InitializeGroup.InitView();
        }

        // 로딩창 비활성화
        if (um.StaticGroup != null)
            um.StaticGroup.panel_Loading.gameObject.SetActive(false);

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
        if (CurServer.type == (int)ServerType.InGame)
        {
            Debug.Log("인게임 서버 단일 룸 사용");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        else if (CurServer.type == (int)ServerType.Lobby || CurServer.type == (int)ServerType.TestServer)
        {
            if (um != null)
            {
                um.LobbyGroup.gameObject.SetActive(true);
                um.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

                um.InitializeGroup.gameObject.SetActive(false);
                um.InGameGroup.gameObject.SetActive(false);

                // 플레이어 정보 업데이트
                if (BackendManager.Auth.CurrentUser != null)
                {
                    //Debug.Log("로비씬 플레이어 정보 갱신");
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdatePlayerInfoView();
                }
                else
                {
                    //Debug.Log("로비씬 플레이어 정보 없으니 게스트 버전 업데이트");
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.ClearView();
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdateGuestInfoView();
                }
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

        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }

        if (CurServer.type == (int)ServerType.InGame)
        {
            if (um != null)
            {
                um.LobbyGroup.gameObject.SetActive(false);
                um.InGameGroup.gameObject.SetActive(true);
                um.InGameGroup.GameStartViewUpdate();

                PlayerManager pm = PlayerManager.Instance;
                if (pm != null)
                {
                    pm.PlayerInstaniate();
                }
            }

        }
        else if (CurServer.type == (int)ServerType.Lobby)
        {
            if (um != null)
            {
                um.LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
                um.LobbyGroup.panel_RoomInside.InitRoomView();
                um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
            }
        }
    }

    // 방 퇴장시 호출됨
    public override void OnLeftRoom()
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }


        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(false);
        }
    }

    // 새로운 플레이어가 방 입장시 호출됨
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    // 다른 플레이어가 방 퇴장시 호출됨
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }


    // 로비에 있을 때 방을 추가 or 삭제할 때 업데이트 됨
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }


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
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }


        if (um != null)
            um.LobbyGroup.panel_RoomInside.panel_MapSettings.UpdateRoomProperty();
    }

    // 방안의 플레이어가 커스텀 프로퍼티가 변경될 때 호출 (다른 사람이 변경해도 호출됨)
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }

        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
    }

    #endregion

    // 서버 이동 처리
    public void ChangeServer(ServerData serverData)
    {
        if (serverData == null) { Debug.LogError("이동할 서버가 설정되지 않음!"); return; }
        
        Hashtable customPropsDB_Player = null;

        // 현재 접속 중인 서버가 없다면 실행 안함
        if (CurServer != null)
        {
            // 플레이어 커스텀 프로퍼티 백업
            if (PhotonNetwork.LocalPlayer.CustomProperties != null)
                customPropsDB_Player = PhotonNetwork.LocalPlayer.CustomProperties;

            // 연결 해제 이전에 서버 퇴장 처리
            BackendManager.Instance.OnExitServerCapacityUpdate(CurServer);

            // 현재 접속 중인 서버 연결 해제
            PhotonNetwork.Disconnect();
        }
        
        // 이동할 서버 AppID 갱신
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;
        // 갱신된 ID의 서버 연결
        PhotonNetwork.ConnectUsingSettings();

        // 연결 후 커스텀 프로퍼티 복원
        if (customPropsDB_Player != null)
            PhotonNetwork.LocalPlayer.CustomProperties = customPropsDB_Player;

        // 연결 후 서버 입장 처리
        BackendManager.Instance.OnEnterServerCapacityUpdate(serverData);

        // 접속에 성공하면 현재 서버 갱신
        CurServer = serverData;
    }

    public void ConnectToBestServer(ServerType serverType)
    {
        BackendManager.Instance.LoadAllTargetTypeServers(serverType, (lobbyServerDic) =>
        {
            BackendManager.Instance.QuickSearchAccessableServer(lobbyServerDic,
            (bestServer) =>
            {
                ChangeServer(bestServer);
            },
            (failMessage) =>
            {
                Debug.LogError($"로비로 접속 실패. 실패 사유 :{failMessage}");
            }
            );
        });
    }

    public void MoveToLobby()
    {
        // 플레이어 인스턴스가 있다면(= 인게임 씬에서 로비로 이동하는 상황이라면) => 플레이어 이벤트 할당 해제
        PlayerManager.Instance?.PlayerToLobby();

        // 로비 서버 중 가장 적합한 서버로 자동 이동
        ConnectToBestServer(ServerType.Lobby);

        // 로비 씬 로드
        PhotonNetwork.LoadLevel(lobbySceneName);
    }

    public void MoveToInGameScene()
    {
        // 테스트 체크 시, 테스트 서버로
        if (isTestServer)
        {
            ConnectToBestServer(ServerType.TestServer);
            PhotonNetwork.LoadLevel(inGameSceneName);
            return;
        }

        ConnectToBestServer(ServerType.InGame);
        PhotonNetwork.LoadLevel(inGameSceneName);
    }

    public void MoveToInGameScene(string targetServerName)
    {
        BackendManager.Instance.GetServerData(targetServerName, ServerType.InGame, (targetServer) => 
        {
            BackendManager.Instance.IsAbleToConnectServer(targetServer, (accessable) => 
            {
                if (accessable)
                {
                    ChangeServer(targetServer);
                }
                else
                {
                    Debug.LogError("이동하려는 서버의 인원이 가득차서 이동할 수 없습니다.");
                }
            });
        });

        PhotonNetwork.LoadLevel(inGameSceneName);
    }

    private void OnApplicationQuit()
    {
        BackendManager.Auth.SignOut();

        if (CurServer != null)
        {
            // 서버 퇴장 처리
            BackendManager.Instance.OnExitServerCapacityUpdate(CurServer);
        }
    }
}



