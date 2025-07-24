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
    [Header("디버깅 용도")]
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
            tmp_State.text = $"Current State : {PhotonNetwork.NetworkClientState}";

        // 테스트용 서버 전환
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
        Debug.Log("연결");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("마스터 연결");

        
        if (um != null)
        {
            // 연결되면 초기화면(UI) 활성화
            um.InitializeGroup.InitView();

            // 로딩창 비활성화
            um.LoadingGroup.fullScreen.gameObject.SetActive(false);
        }
            
    }

    // 로비 입장시 호출됨
    public override void OnJoinedLobby() 
    {
        if (um != null)
        {
            um.LobbyGroup.gameObject.SetActive(true);
            um.InitializeGroup.gameObject.SetActive(false);
            um.InGameGroup.gameObject.SetActive(false);
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

        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.InitRoomView();
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList();
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
        }
    }

    // 방 퇴장시 호출됨
    public override void OnLeftRoom() 
    {
        if (um != null)
        {
            um.LobbyGroup.panel_RoomInside.gameObject.SetActive(false);
        }
    }       

    // 새로운 플레이어가 방 입장시 호출됨
    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList(); 
    }

    // 다른 플레이어가 방 퇴장시 호출됨
    public override void OnPlayerLeftRoom(Player otherPlayer) 
    {
        if (um != null)
            um.LobbyGroup.panel_RoomInside.UpdatePlayerList(); 
    }


    // 로비에 있을 때 방을 추가 or 삭제할 때 업데이트 됨
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log($"정보가 갱신된 방 개수{roomList.Count}");

        // 변화한 애들 현재 방 해시테이블에 갱신
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                Debug.Log($"방 삭제 {info.Name}");
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        Debug.Log($"현재 존재하는 방 개수{cachedRoomList.Count}");

        // 해시테이블을 리스트로 변환
        List<RoomInfo> activedRoomList = cachedRoomList.Values.ToList();

        // 활성화된 방(삭제되지 않은 방)만 업데이트
        UIManager.Instance.LobbyGroup.panel_MatchMaking.UpdateRoomListView(activedRoomList);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (um != null)
            um.LobbyGroup.panel_RoomInside.panel_MapSettings.UpdateRoomProperty();
    }

    // 방안의 플레이어가 커스텀 프로퍼티가 변경될 때 호출 (다른 사람이 변경해도 호출됨)
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
    public string name;
    public string id;
}