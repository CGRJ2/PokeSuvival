using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class NetworkManager : SingletonPUN<NetworkManager>
{
    [Header("디버깅 용도")]
    [SerializeField] TMP_Text tmp_State;

    [SerializeField] private float backendInitTimeoutSec = 10f;

    public ServerData CurServer { get; private set; }

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public UnityEvent<bool> LoadingEvent;
    public UnityEvent PlayerFirstEnterEvent;
    public UnityEvent InGameEnterEvent;
    public UnityEvent LobbyEnterEvent;
    public UnityEvent RoomEnterEvent;
    public UnityEvent RoomUpdateEvent;

    private Hashtable _pendingCustomProps = null;

    public void Init()
    {
        base.SingletonInit();

        Debug.Log("초기화 진행. 로비 서버로 연결 시작");
        StartCoroutine(InitLobbyServerAfterBackendInitComplete());

        // 연결 시도와 동시에 로딩창으로 가리기
        LoadingEvent?.Invoke(true);
    }
    System.Collections.IEnumerator InitLobbyServerAfterBackendInitComplete()
    {
        float start = Time.realtimeSinceStartup;

        // 1) 초기화 완료까지 대기(Timeout 포함)
        while (!BackendManager.IsInitDone)
        {
            if (Time.realtimeSinceStartup - start > backendInitTimeoutSec)
            {
                Debug.LogError($"Firebase Init Timeout ({backendInitTimeoutSec}s)");
                LoadingEvent?.Invoke(false); // 로딩 해제 (또는 에러 UI로 전환)
                yield break;
            }
            yield return null;
        }

        // 2) 초기화 실패 처리
        if (!BackendManager.IsInitSuccess || BackendManager.Auth == null || BackendManager.Database == null)
        {
            // TODO: 타이틀 씬으로 다시 이동

            Debug.LogError($"Firebase Init Failed: {BackendManager.InitFailReason}");
            LoadingEvent?.Invoke(false); // 로딩 해제
            yield break;
        }

        // 3) 로비 서버로 연결
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
                //tmp_State.text = $"현재 서버 : {CurServer.name}, Current State : {PhotonNetwork.NetworkClientState}";
                tmp_State.text = $"현재 서버 : {CurServer.name}";
            else
                tmp_State.text = "현재 접속된 서버 없음";
        }
    }

    public void CheckServerUserNumber_InRoomMasterClient()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            BackendManager.Instance.UpdateServerUserCount(CurServer);
        }
    }

    public void CheckServerUserNumber_LobbyAnyClient()
    {
        BackendManager.Instance.UpdateServerUserCount(CurServer);
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

        // 커스텀 프로퍼티 복원
        if (_pendingCustomProps != null)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(_pendingCustomProps);
            _pendingCustomProps = null;
        }

        // 현재 접속한 서버가 로비라면
        if (CurServer.type == (int)ServerType.Lobby)
        {
            CheckServerUserNumber_LobbyAnyClient();

            // 플레이어 정보가 없으면(= 처음 시작한 상태라면) => InitializeGroup(UI) 활성화
            if (PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty())
            {
                PlayerFirstEnterEvent?.Invoke();

                // 로딩창 비활성화
                LoadingEvent?.Invoke(false);
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
            // 인게임 서버 단일 룸 사용
            string roomName = "UniversalRoom";
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 20,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }

        else if (CurServer.type == (int)ServerType.Lobby)
        {
            LobbyEnterEvent?.Invoke();

            // 로딩창 비활성화
            LoadingEvent?.Invoke(false);
        }
    }

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
            UIManager.Instance.StaticGroup.panel_CustomBGM.RandomAudioClipPlay();

            CheckServerUserNumber_InRoomMasterClient();

            InGameEnterEvent?.Invoke();

            LoadingEvent?.Invoke(false);
        }
        else if (CurServer.type == (int)ServerType.Lobby)
        {
            RoomEnterEvent?.Invoke();
        }
    }

    // 방 퇴장시 호출됨
    public override void OnLeftRoom()
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }
        
        LoadingEvent?.Invoke(true);
    }

    // 새로운 플레이어가 방 입장시 호출됨
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { Debug.LogWarning("인원 업데이트: 새로운 유저 진입"); CheckServerUserNumber_InRoomMasterClient(); return; }

        RoomUpdateEvent?.Invoke();
    }

    // 다른 플레이어가 방 퇴장시 호출됨
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { Debug.LogWarning("인원 업데이트: 유저 이탈"); CheckServerUserNumber_InRoomMasterClient(); return; }

        RoomUpdateEvent?.Invoke();
    }


    // 로비에 있을 때 방을 추가 or 삭제할 때 업데이트 됨
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        if (CurServer.type == (int)ServerType.FunctionTestServer) { return; }
        if (CurServer.type == (int)ServerType.InGame) { return; }

        // 변화한 애들 현재 방 해시테이블에 갱신
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
                cachedRoomList.Remove(info.Name);
            else
                cachedRoomList[info.Name] = info;
        }

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

        RoomUpdateEvent?.Invoke();

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

        RoomUpdateEvent?.Invoke();
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

    public void UpdateUserDataToClient(UserData userData)
    {
        PhotonNetwork.NickName = userData.name;  // 포톤 닉네임에 기존에 생성했던 firebase 닉네임 할당

        Hashtable playerProperty = new Hashtable();
        
        // 로그인 유저라면
        if (BackendManager.Auth.CurrentUser != null)
        {
            if (userData.name != BackendManager.Auth.CurrentUser.DisplayName)
                BackendManager.Instance.UpdateUserProfile(userData.name);
        }
        // 게스트 유저라면
        else { }

        // 유저 데이터 동기화 해주기 
        playerProperty["Id"] = userData.userId;
        playerProperty["StartingPokemon"] = userData.startingPokemonName;
        playerProperty["Money"] = userData.money;
        playerProperty["Kills"] = userData.kills;
        playerProperty["Level"] = userData.level;
        playerProperty["SuvivalTime"] = userData.survivalTime;
        playerProperty["HighScore"] = userData.highScore;
        int[] ownedItemIds = userData.ownedItemList.ToArray();
        playerProperty["OwnedItems"] = ownedItemIds;
        playerProperty["HeldItem"] = userData.heldItem;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    private void ChangeServerInternal(ServerData serverData, bool skipCapacityCheck)
    {
        if (serverData == null)
        {
            Debug.LogError("이동할 서버가 설정되지 않음!");
            return;
        }

        void DoSwitch()
        {
            // 플레이어 커스텀 프로퍼티 백업
            var props = PhotonNetwork.LocalPlayer?.CustomProperties;
            if (props != null)
            {
                _pendingCustomProps = new Hashtable();
                foreach (var kv in props)
                    _pendingCustomProps[kv.Key] = kv.Value;
            }

            // 로딩 UI
            LoadingEvent?.Invoke(true);

            // 기존 연결 종료 (연결 중/미연결 상태여도 호출은 안전)
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();

            // AppId 교체
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = serverData.id;

            // 현재 서버 갱신 (문서에서도 "전환 목표 서버"로 사용 가능)
            CurServer = serverData;

            // 재연결
            PhotonNetwork.ConnectUsingSettings();
        }

        if (skipCapacityCheck)
        {
            DoSwitch();
            return;
        }

        // 일반 전환: 접속 가능 여부 확인
        BackendManager.Instance.IsAbleToConnectServer(serverData, accessable =>
        {
            if (accessable) DoSwitch();
            else Debug.LogError("해당 서버 접속 불가능. 사유: 인원 초과");
        });
    }


    // 서버 이동 처리
    public void ChangeServer(ServerData serverData)
    {
        ChangeServerInternal(serverData, skipCapacityCheck: false);
    }

    // 예약된 이동 처리 (반드시 성공)
    public void ReservedChangeServer(ServerData serverData)
    {
        ChangeServerInternal(serverData, skipCapacityCheck: true);
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


        // 접속 종료 시 해당 플레이어 킬
        PlayerController pc = PlayerManager.Instance?.LocalPlayerController;

        if (pc == null) return;
        if (pc.Model == null) return;

        if (!pc.Model.IsDead)
            pc.Model.SetCurrentHp(-1);
    }

    public override void OnDisable()
    {
        LoadingEvent.RemoveAllListeners();
        PlayerFirstEnterEvent.RemoveAllListeners();
        InGameEnterEvent.RemoveAllListeners();
        LobbyEnterEvent.RemoveAllListeners();
        RoomEnterEvent.RemoveAllListeners();
        RoomUpdateEvent.RemoveAllListeners();
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 에디터 모드 종료
#else
    Application.Quit(); // 빌드 시 실제 종료
#endif
    }

}



