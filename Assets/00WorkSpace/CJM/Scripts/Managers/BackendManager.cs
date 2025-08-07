using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BackendManager : Singleton<BackendManager>
{
    public static FirebaseApp App { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    // 테스트용
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            //LoadUserDataFromDB((data) => Debug.Log(data.name));
            Debug.Log(NetworkManager.Instance.CurServer.sceneName);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            //InitServerDataToServerInfoDB(new ServerData("Lobby Server 01 (KR)", "LobbyScene(CJM)", "로비 01", 0, "e4d01a07-2d0c-41bb-bc2d-59723abc27fc", 20));
            //InitServerDataToServerInfoDB(new ServerData("Lobby Server 02 (KR)", "LobbyScene(CJM)", "로비 02", 0, "4b17f092-1646-4668-9356-580cdb2e8529", 20));
        }
    }

    public void Init()
    {
        base.SingletonInit();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("파이어 베이스 설정이 모두 충족되어 사용할 수 있는 상황");
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;
            }
            else
            {
                Debug.LogError($"파이어 베이스 설정이 충족되지 않아 실패했습니다. 이유: {dependencyStatus}");
                App = null;
                Auth = null;
                Database = null;
            }
        });
    }

    // DB에 데이터 존재 여부 판단
    public void CheckData(DatabaseReference dbRef, Action<bool> onChecked = null)
    {
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("데이터 존재 여부 확인 실패");
                onChecked?.Invoke(false); // 에러도 false 처리
                return;
            }

            DataSnapshot snapshot = task.Result;
            bool exists = snapshot.Exists;

            Debug.Log($"데이터 존재 여부: {exists}");
            onChecked?.Invoke(exists);
        });
    }

    #region User Data 관리

    // Auth - 로그인 유저 프로필 업데이트
    public void UpdateUserProfile(string name)
    {
        FirebaseUser user = Auth.CurrentUser;
        if (user != null)
        {
            UserProfile profile = new UserProfile
            {
                DisplayName = name,
                //PhotoUrl = new System.Uri("https://example.com/jane-q-user/profile.jpg"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("유저 닉네임 설정 취소됨.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"유저 닉네임 설정 실패함. 실패사유: {task.Exception}, ErrorCode: {((FirebaseException)task.Exception.InnerException).ErrorCode}");

                    return;
                }

                Debug.Log("유저 프로필이 성공적으로 업데이트 됨");
            });
        }
    }

    // DB - UserData 생성(SetRawJsonValueAsync)
    public void InitUserDataToDB(UserData data, Action onSuccess = null, Action<string> onFail = null)
    {
        Debug.Log("DB에 UserData 첫 생성");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UserData 저장 작업이 취소되었습니다.");
                onFail?.Invoke("작업 취소됨");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"UserData 저장 실패: {task.Exception}");
                onFail?.Invoke("저장 실패");
                return;
            }

            Debug.Log("UserData 저장 성공");
            onSuccess?.Invoke();
        });
    }

    // DB - UserData 내부 값 변경(UpdateChildrenAsync)
    public void UpdateUserDataValue(string key, object value)
    {
        Debug.Log($"UserData의 {key} 데이터 변경");

        string userId = Auth.CurrentUser.UserId;

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic[key] = value;

        userInfo.UpdateChildrenAsync(dic);
    }


    // DB - Auth.CurrentUser.UserId를 키값으로 유저 데이터 불러오기(GetValueAsync)
    public void LoadUserDataFromDB(Action<UserData> onSuccess = null, Action<string> onFail = null)
    {
        string userId = Auth.CurrentUser.UserId;
        DatabaseReference userInfo = Database.RootReference.Child("UserData").Child(userId);

        userInfo.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("유저 데이터 불러오기 취소됨.");
                onFail?.Invoke("취소 메시지 전달용");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"유저 데이터 불러오기 실패: {task.Exception}");
                onFail?.Invoke("실패 메시지 전달용");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                UserData data = JsonUtility.FromJson<UserData>(json);
                Debug.Log($"유저 데이터 불러오기 성공: {json}");
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("유저 데이터가 존재하지 않음.");
                onFail?.Invoke("유저 데이터가 없다는 메시지 전송");
            }
        });

    }

    #endregion

    #region Server Data 관리

    // 서버 종류별 DatabaseReference Root 지정
    public DatabaseReference GetServerBaseRef(ServerType serverType)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference;

        switch (serverType)
        {
            case ServerType.Lobby:
                reference = root.Child("Server Info").Child("Lobby Servers");
                break;

            case ServerType.InGame:
                reference = root.Child("Server Info").Child("InGame Servers");
                break;

            default:
                reference = root.Child("Server Info").Child("Test-Servers");
                break;
        }

        return reference;
    }

    // DB - Server Data 생성(SetRawJsonValueAsync)
    public void InitServerDataToServerInfoDB(ServerData data)
    {
        Debug.Log("DB에 해당 Server Data의 Server Info 첫 생성");

        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)data.type).Child(data.key);

        ServerData serverInfo = new ServerData(data.key, data.sceneName, data.name, data.type, data.id, 20);
        string json = JsonUtility.ToJson(serverInfo);
        reference.SetRawJsonValueAsync(json);
    }

    // 서버 입장 시, 서버 인원에 본인 추가
    /*public void OnEnterServerCapacityUpdate(ServerData curServerData, List<string> memberIdList, Action onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.Child("curPlayerList").RunTransaction(mutableData =>
        {
            //Debug.LogWarning($"왜 이거 자꾸 null만 뜨지? 서버에 데이터 있는데? {mutableData.Value}");
            if (mutableData.Value == null)
            {
                Debug.Log("curPlayerList가 없어서 새로 생성함");
                mutableData.Value = memberIdList.Cast<object>().ToList();
                return TransactionResult.Success(mutableData);
            }
            else
            {
                try
                {
                    // Firebase에서는 object 리스트로 반환됨
                    var existingList = new List<string>();
                    foreach (var item in (IEnumerable)mutableData.Value)
                    {
                        if (item != null)
                            existingList.Add(item.ToString());
                    }

                    List<string> curUserList = existingList.Select(o => o.ToString()).ToList();

                    Debug.Log("현재 유저 리스트에 추가함");
                    foreach (string id in memberIdList)
                    {
                        if (!curUserList.Contains(id))
                            curUserList.Add(id);
                        else Debug.LogError($"이미 서버에 존재하는 Id를 또 추가하려고함: {id}");
                    }
                    
                    mutableData.Value = curUserList.Cast<object>().ToList(); // Firebase는 object 리스트로 저장됨
                    return TransactionResult.Success(mutableData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"서버 인원 추가 실패 - 형변환 오류: {e.Message}");
                    return TransactionResult.Abort();
                }
            }
        });
    }*/

    // 서버 퇴장 시, 서버 인원에 본인 제거
    /*public void OnExitServerCapacityUpdate(ServerData curServerData, string userId, Action onSucess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.Child("curPlayerList").RunTransaction(mutableData =>
        {
            try
            {
                if (mutableData.Value == null)
                {
                    // 아무 리스트도 없다면 빈 리스트로 초기화
                    mutableData.Value = new List<object>();
                    onSucess?.Invoke();
                    return TransactionResult.Success(mutableData);
                }

                // Firebase에서는 object 리스트로 반환됨
                var existingList = new List<string>();
                foreach (var item in (IEnumerable)mutableData.Value)
                {
                    if (item != null)
                        existingList.Add(item.ToString());
                }
                List<string> curUserList = existingList.Select(o => o.ToString()).ToList();


                // 현재 유저 ID 제거
                if (curUserList.Contains(userId))
                    curUserList.Remove(userId);
                else Debug.LogError($"서버에 제거하려는 Id가 존재하지 않음: {userId}");

                Debug.Log($"현재 유저 리스트에서 제거 후 새 리스트 Count: {curUserList.Count}");


                // 다시 object 리스트로 저장
                if (curUserList.Count < 1)
                    mutableData.Value = null;
                else
                    mutableData.Value = curUserList.Cast<object>().ToList();

                onSucess?.Invoke();
                return TransactionResult.Success(mutableData);
            }
            catch (Exception e)
            {
                Debug.LogError($"서버 인원 제거 실패 - 형변환 오류: {e.Message}");
                onFail?.Invoke("인원 제거 중 오류 발생");
                return TransactionResult.Abort();
            }
        });
    }*/

    // 서버 데이터로, 현재 접속 가능한지 여부 판단
    public void IsAbleToConnectServer(ServerData curServerData, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("서버 데이터 불러오기 취소됨.");
                onFail?.Invoke("취소 메시지 전달용");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"서버 데이터 불러오기 실패: {task.Exception}");
                onFail?.Invoke("실패 메시지 전달용");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                long curPlayerCount = (long)snapshot.Child("curPlayerCount").Value;
                long maxPlayerCount = (long)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"서버 데이터 불러오기 성공. 현재 인원 / 최대 인원: {curPlayerCount}/{maxPlayerCount}");

                if (curPlayerCount < maxPlayerCount)
                    onSuccess?.Invoke(true);
                else
                    onSuccess?.Invoke(false);
            }
            else
            {
                Debug.LogWarning("해당 서버가 데이터베이스에 존재하지 않음.");
                onFail?.Invoke("해당 서버가 데이터베이스에 없다는 메시지 전송");
            }
        });
    }

    public void CheckMultipleUsersSpaceAndReserve(ServerData curServerData, int multiUserCount, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);


        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {

            if (task.IsCanceled)
            {
                Debug.LogError("서버 데이터 불러오기 취소됨.");
                onFail?.Invoke("취소 메시지 전달용");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"서버 데이터 불러오기 실패: {task.Exception}");
                onFail?.Invoke("실패 메시지 전달용");
                return;
            }

            DataSnapshot snapshot = task.Result;

            
            if (snapshot.Exists)
            {
                long curPlayerCount = (long)snapshot.Child("curPlayerCount").Value;
                long maxPlayerCount = (long)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"서버 데이터 불러오기 성공. 현재 인원 / 최대 인원: {curPlayerCount}/{maxPlayerCount}");

                // 자리가 있으면 예약해주기
                if (curPlayerCount < maxPlayerCount - multiUserCount + 1)
                {
                    reference.Child("reservedPlayerCount").RunTransaction(mutableData =>
                    {
                        if (mutableData.Value == null)
                        {
                            mutableData.Value = (long)multiUserCount;
                            return TransactionResult.Success(mutableData);
                        }
                        else
                        {
                            try
                            {
                                long currentReservedUserCount = (long)mutableData.Value;
                                mutableData.Value = currentReservedUserCount + (long)multiUserCount;
                                return TransactionResult.Success(mutableData);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"서버 인원 갱신 실패 - 형변환 오류: {e.Message}");
                                return TransactionResult.Abort();
                            }
                        }
                    }).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.LogError("접근하려는 서버에 자리는 있는데 자리 예약에 실패함!");
                            onSuccess?.Invoke(false);
                        }
                        // 자리 예약 완료되면 실행
                        onSuccess?.Invoke(true);
                    });
                }
                // 없으면 false 반환
                else
                    onSuccess?.Invoke(false);
            }
            else
            {
                Debug.LogWarning("해당 서버가 데이터베이스에 존재하지 않음.");
                onFail?.Invoke("해당 서버가 데이터베이스에 없다는 메시지 전송");
            }
        });
    }

    // 서버 타입+이름으로 해당 서버 데이터 반환하기
    public void GetServerData(string key, ServerType type, Action<ServerData> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference serverRoot = GetServerBaseRef(type);
        DatabaseReference targetRef = serverRoot.Child(key);

        targetRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("서버 불러오기 실패");
                onFail?.Invoke("불러오기 실패");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                ServerData data = JsonUtility.FromJson<ServerData>(json);
                Debug.Log($"서버 데이터 불러오기 성공: {json}");

                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("서버 데이터가 존재하지 않음.");
                onFail?.Invoke("서버 데이터가 없다는 메시지 전송");
            }

        });
    }

    // 해당 종류의 모든 서버 반환 (ex. type = InGameServer 라면 모든 인게임 서버를 딕셔너리로 가져옴)
    public void LoadAllTargetTypeServers(ServerType type, Action<Dictionary<string, ServerData>> onSuccess = null, Action<string> onFail = null)
    {
        // 해당 타입의 저장소 접근
        DatabaseReference serverRoot = GetServerBaseRef(type);
        Debug.Log($"서버들 불러오기... 타입:{type}");
        serverRoot.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("서버 리스트 불러오기 실패");
                onFail?.Invoke("불러오기 실패");
                return;
            }

            DataSnapshot snapshot = task.Result;
            var serverDict = new Dictionary<string, ServerData>();

            foreach (var child in snapshot.Children)
            {
                string key = child.Key; // ex) "In Game Server 01 (KR)"
                string json = child.GetRawJsonValue();
                //Debug.Log($"서버 JSON: {json}");
                ServerData data = JsonUtility.FromJson<ServerData>(json);

                serverDict[key] = data;
            }

            onSuccess?.Invoke(serverDict);
        });
    }

    // 서버 리스트 중에 1. 가장 인원이 많고, 2. 접속 가능한 서버를 반환
    public void QuickSearchAccessableServer(Dictionary<string, ServerData> serverDic, Action<ServerData> onSuccess, Action<string> onFail = null)
    {
        Debug.Log("베스트 서버 찾기");


        if (serverDic == null || serverDic.Count == 0)
        {
            onFail?.Invoke("서버 딕셔너리가 비어 있음");
            return;
        }

        ServerData bestServer = null;
        int highestPlayerCount = -1;

        foreach (var kvp in serverDic)
        {
            ServerData server = kvp.Value;

            if (server.curPlayerCount < server.maxPlayerCount)
            {
                if (server.curPlayerCount > highestPlayerCount)
                {
                    highestPlayerCount = server.curPlayerCount;
                    bestServer = server;
                }
            }
        }

        if (bestServer != null)
        {
            Debug.Log($"QuickSearch 결과: {bestServer.name} (접속 가능 인원: {bestServer.curPlayerCount}/{bestServer.maxPlayerCount})");
            onSuccess?.Invoke(bestServer);
        }
        else
        {
            Debug.LogWarning("접속 가능한 서버가 없음");
            onFail?.Invoke("접속 가능한 서버 없음");
        }
    }

    public void UpdateServerUserCount(ServerData curServerData, Action onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.Child("curPlayerCount").RunTransaction(mutableData =>
        {
            //Debug.LogWarning($"왜 이거 자꾸 null만 뜨지? 서버에 데이터 있는데? {mutableData.Value}");
            if (mutableData.Value == null)
            {
                long serverUserCount = PhotonNetwork.CountOfPlayersOnMaster + PhotonNetwork.CountOfPlayersInRooms;
                mutableData.Value = serverUserCount;
                return TransactionResult.Success(mutableData);
            }
            else
            {
                try
                {
                    long serverUserCount = PhotonNetwork.CountOfPlayersOnMaster + PhotonNetwork.CountOfPlayersInRooms;
                    mutableData.Value = serverUserCount; 
                    return TransactionResult.Success(mutableData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"서버 인원 갱신 실패 - 형변환 오류: {e.Message}");
                    return TransactionResult.Abort();
                }
            }
        });
    }



    #endregion

    #region 랭킹 시스템

    // 유저 랭킹 데이터 생성 (로그인 or 게스트 무관 => 그냥 점수랑 유저 아이디, 닉네임만 있으면 됨)
    public void InitLocalPlayerRankingData(RankData rankData)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(rankData.userId);

        string json = JsonUtility.ToJson(rankData);
        reference.SetRawJsonValueAsync(json);
    }

    // 유저 랭킹 데이터 불러오기 (점수 비교용)
    public void LoadLocalPlayerRankData(string userId, Action<RankData> onSuccess = null, Action<string> onFail = null)
    {
        //Debug.LogWarning("랭킹 데이터 받아오기");

        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(userId);

        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("랭킹 데이터 불러오기 취소됨.");
                onFail?.Invoke("취소 메시지 전달용");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"랭킹 데이터 불러오기 실패: {task.Exception}");
                onFail?.Invoke("실패 메시지 전달용");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                RankData data = JsonUtility.FromJson<RankData>(json);
                Debug.Log($"랭킹 데이터 불러오기 성공: {json}");
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("랭킹 데이터가 존재하지 않음.");
                onFail?.Invoke("랭킹 데이터가 없다는 메시지 전송");
            }
        });

    }

    // 유저 신기록을 해당 유저의 랭킹 데이터에 업데이트
    public void UpdateHighScore(int newScore, string UserId)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(UserId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        
        dic["name"] = PhotonNetwork.NickName; //닉네임을 변경했을 때 적용하는 걸 대비
        dic["highScore"] = newScore;

        reference.UpdateChildrenAsync(dic);
    }

    // 랭킹 데이터를 정렬해서 1 ~ 10위까지 반환
    public void UpdateRankingBoard_SortedByScore(Action<List<KeyValuePair<string, RankData>>> onSuccess = null)
    {
        LoadAllRankData((dic) =>
        {
            // highScore 기준 내림차순 정렬
            var top10List = dic
                .OrderByDescending(kvp => kvp.Value.highScore)
                .Take(10)
                .ToList();

            //Debug.Log("랭킹 Top 10:");
            for (int i = 0; i < top10List.Count; i++)
            {
                var entry = top10List[i];
                //Debug.Log($"{i + 1}위 - {entry.Value.userName} / 점수: {entry.Value.highScore}");
            }

            onSuccess?.Invoke(top10List);
        });
    }

    // 본인의 순위 받아오기
    public void GetRankNumb(string userId,Action<int> onSuccess = null, Action<string> onFail = null)
    {
        //Debug.LogWarning("랭크 순위 받아오기");
        LoadAllRankData((dic) =>
        {
            // highScore 기준 내림차순 정렬
            var sortedList = dic.OrderByDescending(kvp => kvp.Value.highScore).ToList();

            for (int i = 0; i < sortedList.Count; i++)
            {
                if (sortedList[i].Value.userId == userId)
                {
                    //Debug.LogWarning("내 데이터 발견, 순위 반환");
                    onSuccess?.Invoke(i);
                    return;
                }
            }
            Debug.LogWarning("내 데이터 발견 못함");
            onFail?.Invoke("랭킹에 내 정보 없음");
        });
    }


    public void LoadAllRankData(Action<Dictionary<string, RankData>> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference serverRoot = root.Child("RankingBoardData");
        serverRoot.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("랭크 데이터들 불러오기 실패");
                onFail?.Invoke("불러오기 실패");
                return;
            }

            DataSnapshot snapshot = task.Result;
            var rankDataDict = new Dictionary<string, RankData>();

            foreach (var child in snapshot.Children)
            {
                string key = child.Key; // => userId
                string json = child.GetRawJsonValue();
                RankData data = JsonUtility.FromJson<RankData>(json);

                rankDataDict[key] = data;
            }

            onSuccess?.Invoke(rankDataDict);
        });
    }
    #endregion
}

[Serializable]
public class UserData
{
    public string name;
    public string userId;
    public int level;
    public int money;
    public int kills;
    public float suvivalTime;
    public float highScore;
    public string startingPokemonName;
    public List<int> owndItemList;
    public int heldItem;


    public UserData(string name, string userId)
    {
        this.name = name;
        this.userId = userId;
        this.level = 1;
        this.money = 0;
        this.kills = 0;
        this.suvivalTime = 0;
        this.highScore = 0;
        this.startingPokemonName = "";
        this.userId = userId;
        owndItemList = new List<int>();
    }
}

[Serializable]
public class ServerData
{
    public string key;
    public string sceneName;
    public string name;
    public string id;
    public int type;
    public int maxPlayerCount;
    public int curPlayerCount;
    public int reservedPlayerCount;
    //public List<string> curPlayerList;

    public ServerData() { }

    public ServerData(string key, string sceneName, string name, int type, string id, int maxPlayerCount)
    {
        this.key = key;
        this.sceneName = sceneName;
        this.name = name;
        this.id = id;
        this.type = type;
        this.maxPlayerCount = maxPlayerCount;
        curPlayerCount = 0;
        reservedPlayerCount = 0;
        //this.curPlayerList = new List<string>();
    }
}
public enum ServerType { Lobby, InGame, TestServer, FunctionTestServer };

[Serializable]
public class RankData
{
    public string userId;
    public string userName;
    public int highScore;

    public RankData(string userId, string userName, int highScore)
    {
        this.userId = userId;
        this.userName = userName;
        this.highScore = highScore;
    }
}

