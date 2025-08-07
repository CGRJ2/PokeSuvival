using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class NetworkManager : SingletonPUN<NetworkManager>
{
    [Header("디버깅 용도")]
    [SerializeField] TMP_Text tmp_State;

    [Header("테스터 서버 연결 여부")]
    [SerializeField] bool isTestServer;
    [SerializeField] int testServerIndex;

    /*[Header("씬 전환을 위한 이름(string) 저장")]
    public string inGameSceneName; // 맵이 여러개라면 서버 데이터에 저장
    public string lobbySceneName;*/

    public ServerData CurServer { get; private set; }

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    UIManager um;

    Action<string> ForcedQuitEvent;

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
        // 이런 WaitUntil로 무한 대기하는 구조들을 콜백 기반으로 리팩토링해주는 작업 필요 (TODO)
        yield return new WaitUntil(() => BackendManager.Auth != null);
        yield return new WaitUntil(() => BackendManager.Database != null);
        //yield return new WaitUntil(() => PhotonNetwork.LocalPlayer != null);
        // 로비 서버로 연결
        Debug.Log("서버 최초 연결 시도");
        ConnectToBestServer(ServerType.Lobby);
    }
    private void OnDestroy() => StopAllCoroutines();


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

        // 테스트 용
        if (Input.GetKeyDown(KeyCode.T))
        {
            //Debug.Log($"Auth CurrentUser => {BackendManager.Auth.CurrentUser.UserId}");
            Debug.Log($"로비에 존재하는 인원 => {PhotonNetwork.CountOfPlayersOnMaster}명, 룸에 존재하는 인원 => {PhotonNetwork.CountOfPlayersInRooms}");
            
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

        // 연결 후 서버 입장 처리
        BackendManager.Instance.OnEnterServerCapacityUpdate(CurServer, new List<string>() { GetUserId() });

        // 현재 접속한 서버가 로비라면
        if (CurServer.type == (int)ServerType.Lobby)
        {
            // 플레이어 정보가 없으면(= 처음 시작한 상태라면) => InitializeGroup(UI) 활성화
            if (PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty())
            {
                um.InitializeGroup.InitView();

                // 로딩창 비활성화
                if (um.StaticGroup != null)
                    um.StaticGroup.panel_Loading.gameObject.SetActive(false);
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



    }

    System.Collections.IEnumerator JoinLobbyAfterConnectedMaster()
    {
        // 이런 WaitUntil로 무한 대기하는 구조들을 콜백 기반으로 리팩토링해주는 작업 필요 (TODO)
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

            string roomName = "UniversalRoom";
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 20,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);

            um.StaticGroup.SetDefaultSettings();
            um.CloseAllActivedPanels();
        }

        else if (CurServer.type == (int)ServerType.Lobby || CurServer.type == (int)ServerType.TestServer)
        {
            if (um != null)
            {
                um.LobbyGroup.gameObject.SetActive(true);
                um.LobbyGroup.OnJoinedLobbyDefaultSetting();
                um.CloseAllActivedPanels();
                um.LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

                um.InitializeGroup.gameObject.SetActive(false);
                um.InGameGroup.gameObject.SetActive(false);

                // 플레이어 정보 업데이트
                if (BackendManager.Auth.CurrentUser != null)
                {
                    //Debug.Log("로비씬 플레이어 정보 갱신");
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdatePlayerInfoView();
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerRecords.UpdateView();
                }
                else
                {
                    //Debug.Log("로비씬 플레이어 정보 없으니 게스트 버전 업데이트");
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.ClearView();
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdateGuestInfoView();
                    um.LobbyGroup.panel_LobbyDefault.panel_PlayerRecords.UpdateView();
                }
            }

            // 로딩창 비활성화
            if (um.StaticGroup != null)
                um.StaticGroup.panel_Loading.gameObject.SetActive(false);
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

            // 로딩창 비활성화
            if (um.StaticGroup != null)
                um.StaticGroup.panel_Loading.gameObject.SetActive(false);
        }
        else if (CurServer.type == (int)ServerType.Lobby)
        {
            if (um != null)
            {
                um.LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
                um.CloseAllActivedPanels();
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
            // 로딩창 활성화
            if (um.StaticGroup != null)
                um.StaticGroup.panel_Loading.gameObject.SetActive(true);

            // 방 패널 끄기
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


        // 아직 안정성이 검증 안됨 : TODO (동시 접속 방해 테스트 필요)
        if (PhotonNetwork.CurrentRoom.CustomProperties["Start"] != null)
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["Start"])
            {
                string selectedMapKey = (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
                BackendManager.Instance.GetServerData(selectedMapKey, ServerType.InGame, (targetServer) =>
                {
                    // 서버에 예약된 자리로 이동
                    ReservedChangeServer(targetServer);

                    // 씬 로드
                    if (SceneManager.GetActiveScene().name != targetServer.sceneName)
                        PhotonNetwork.LoadLevel(targetServer.sceneName);
                });
            }
        }
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

    // 방안의 마스터 클라이언트 변경시 호출됨
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer != newMasterClient)
            return;

        if (CurServer.type == (int)ServerType.Lobby)
        {
            UIManager.Instance.LobbyGroup.panel_RoomInside.panel_MapSettings.MasterClientViewUpdate(true);
        }
    }
    #endregion

    public void UpdateUserDataToClient(UserData userData)
    {
        PhotonNetwork.NickName = userData.name;  // 포톤 닉네임에 기존에 생성했던 firebase 닉네임 설정

        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        // 로그인 유저라면
        if (BackendManager.Auth.CurrentUser != null)
        {
            BackendManager.Instance.UpdateUserProfile(userData.name);
        }
        // 게스트 유저라면
        else
        {

        }

        // TODO: 보유 아이템 & 장착 아이템 저장 및 동기화 필요
        // 유저 데이터 동기화 해주기 
        playerProperty["Id"] = userData.userId;
        playerProperty["StartingPokemon"] = userData.startingPokemonName;
        playerProperty["Money"] = userData.money;
        playerProperty["Kills"] = userData.kills;
        playerProperty["Level"] = userData.level;
        playerProperty["SuvivalTime"] = userData.suvivalTime;
        playerProperty["HighScore"] = userData.highScore;
        int[] ownedItemIds = userData.owndItemList.ToArray();
        playerProperty["OwnedItems"] = ownedItemIds;
        playerProperty["HeldItem"] = userData.heldItem;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    // 서버 이동 처리
    public void ChangeServer(ServerData serverData)
    {
        if (serverData == null) { Debug.LogError("이동할 서버가 설정되지 않음!"); return; }

        // 해당 서버 접속 가능여부 판단
        BackendManager.Instance.IsAbleToConnectServer(serverData, (accessable) =>
        {
            //Debug.Log($"접근 가능은 한가?{accessable}");
            if (accessable)
            {
                Hashtable customPropsDB_Player = null;

                // 현재 접속 중인 서버가 없다면 실행 안함
                if (CurServer != null)
                {
                    // 플레이어 커스텀 프로퍼티 백업
                    if (PhotonNetwork.LocalPlayer.CustomProperties != null)
                        customPropsDB_Player = PhotonNetwork.LocalPlayer.CustomProperties;

                    // 연결 해제 이전에 서버 퇴장 처리
                    Debug.LogWarning("퇴장처리 진행 해주니?");
                    BackendManager.Instance.OnExitServerCapacityUpdate(CurServer, GetUserId());

                    // 로딩창 활성화
                    if (um.StaticGroup != null)
                        um.StaticGroup.panel_Loading.gameObject.SetActive(true);

                    // 현재 접속 중인 서버 연결 해제
                    PhotonNetwork.Disconnect();
                }

                // 이동할 서버 AppID 갱신
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;
                // 갱신된 ID의 서버 연결

                // 접속에 성공하면 현재 서버 갱신
                CurServer = serverData;

                PhotonNetwork.ConnectUsingSettings();

                // 연결 후 커스텀 프로퍼티 복원
                if (customPropsDB_Player != null)
                    PhotonNetwork.LocalPlayer.CustomProperties = customPropsDB_Player;
            }
            else
            {
                Debug.LogError("해당 서버 접속 불가능. 사유: 인원 초과");
            }
        });
    }

    // 예약된 이동 처리 (반드시 성공)
    public void ReservedChangeServer(ServerData serverData)
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
            BackendManager.Instance.OnExitServerCapacityUpdate(CurServer, GetUserId());

            // 로딩창 활성화
            if (um.StaticGroup != null)
                um.StaticGroup.panel_Loading.gameObject.SetActive(true);

            // 현재 접속 중인 서버 연결 해제
            PhotonNetwork.Disconnect();
        }

        // 이동할 서버 AppID 갱신
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;
        // 갱신된 ID의 서버 연결

        // 접속에 성공하면 현재 서버 갱신
        CurServer = serverData;

        PhotonNetwork.ConnectUsingSettings();

        // 연결 후 커스텀 프로퍼티 복원
        if (customPropsDB_Player != null)
            PhotonNetwork.LocalPlayer.CustomProperties = customPropsDB_Player;
    }

    // 해당 타입 서버들 중 최적 서버를 찾아 서버 이동(& 씬 이동)
    public void ConnectToBestServer(ServerType serverType)
    {
        BackendManager.Instance.LoadAllTargetTypeServers(serverType, (lobbyServerDic) =>
        {
            BackendManager.Instance.QuickSearchAccessableServer(lobbyServerDic,
            (bestServer) =>
            {
                ChangeServer(bestServer);

                if (string.IsNullOrEmpty(bestServer.sceneName))
                {
                    Debug.LogError($"씬 이동 실패. 실패 사유: 서버에서 전달받은 씬 이름이 비어있습니다! 씬 이름이 비어 있는 서버 데이터: {bestServer.name}");
                    return;
                }

                if (Application.CanStreamedLevelBeLoaded(bestServer.sceneName))
                {
                    // 씬 로드
                    if (SceneManager.GetActiveScene().name != bestServer.sceneName)
                        PhotonNetwork.LoadLevel(bestServer.sceneName);
                }
                else
                {
                    Debug.LogError($"씬 {bestServer.sceneName} 가 Build Settings에 없습니다.");
                }
            },
            (failMessage) =>
            {
                Debug.LogError($"로비로 접속 실패. 실패 사유 :{failMessage}");
            }
            );
        });
    }

    // 로비 서버 중 최적의 로비 서버로 이동 (인게임 씬에서 로비로 이동할 때 & 처음 게임을 실행할 때 예외처리)
    public void MoveToLobby()
    {
        // 플레이어 인스턴스가 있다면(= 인게임 씬에서 로비로 이동하는 상황이라면) => 플레이어 이벤트 할당 해제
        PlayerManager.Instance?.PlayerToLobby();

        // 로비 서버 중 가장 적합한 서버로 자동 이동
        ConnectToBestServer(ServerType.Lobby);
    }

    // 인게임 서버 중 최적의 서버로 이동(퀵매치 전용)
    public void MoveToInGameScene()
    {
        // 테스트 체크 시, 테스트 서버로
        if (isTestServer)
        {
            ConnectToBestServer(ServerType.TestServer);
            return;
        }

        ConnectToBestServer(ServerType.InGame);
    }

    // 인게임 서버 중 타겟 키에 해당하는 서버로 이동(인게임 서버 선택 이동 전용)
    public void MoveToInGameScene(string targetServerKey)
    {
        BackendManager.Instance.GetServerData(targetServerKey, ServerType.InGame, (targetServer) =>
        {
            BackendManager.Instance.IsAbleToConnectServer(targetServer, (accessable) =>
            {
                if (accessable)
                {
                    ChangeServer(targetServer);

                    // 씬 로드
                    if (SceneManager.GetActiveScene().name != targetServer.sceneName)
                        PhotonNetwork.LoadLevel(targetServer.sceneName);
                }
                else
                {
                    Debug.LogError("이동하려는 서버의 인원이 가득차서 이동할 수 없습니다.");
                }
            });
        });
    }


    public string GetUserId()
    {
        if (BackendManager.Auth.CurrentUser != null)
        {
            return $"{BackendManager.Auth.CurrentUser.UserId}";
        }
        else
        {
            return $"Guest({PhotonNetwork.LocalPlayer.UserId})";
        }
    }

    // 앱 종료 시점에서 비동기 작업을 실행해서 오류가 나는 듯 함
    private void OnApplicationQuit()
    {
        BackendManager.Auth.SignOut();

        //if (CurServer != null)
        //{
        //    // 서버 퇴장 처리
        //    BackendManager.Instance.OnExitServerCapacityUpdate(CurServer);
        //}
    }

    public void GameQuit()
    {
        BackendManager.Instance.OnExitServerCapacityUpdate(CurServer, GetUserId(), () =>
        {
            Debug.Log("게임 종료 시도");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false; // 에디터 모드 종료
#else
    Application.Quit(); // 빌드 시 실제 종료
#endif
        });
    }

}



