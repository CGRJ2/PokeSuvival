using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using System;
using System.Collections.Generic;
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
            LoadUserDataFromDB((data) => Debug.Log(data.name));
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
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

    #region User Data

    // Auth - 유저 프로필 업데이트
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
    public void InitUserDataToDB(UserData data)
    {
        Debug.Log("DB에 UserData 첫 생성");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json);
    }

    // DB - UserData 내부 값 변경(UpdateChildrenAsync)
    public void UpdateUserData(string key, object value)
    {
        Debug.Log($"UserData의 {key} 데이터 변경");

        string userId = Auth.CurrentUser.UserId;

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic[key] = value;

        userInfo.UpdateChildrenAsync(dic);
    }

    // DB - 유저 데이터 불러오기(GetValueAsync)
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

    #region Server State Data

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
        DatabaseReference reference = GetServerBaseRef((ServerType)data.type).Child(data.name);

        ServerData serverInfo = new ServerData(data.name, data.id, 20);
        string json = JsonUtility.ToJson(serverInfo);
        reference.SetRawJsonValueAsync(json);
    }

    // 서버 입장 시, 서버 인원에 본인 추가
    public void OnEnterServerCapacityUpdate(ServerData curServerData, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.name);
        
        reference.Child("curPlayerCount").RunTransaction(mutableData =>
        {
            if (mutableData.Value == null)
            {
                mutableData.Value = 1;
                return TransactionResult.Success(mutableData);
            }
            else
            {
                long curUserCount = (long)mutableData.Value;
                mutableData.Value = curUserCount + 1;
                return TransactionResult.Success(mutableData);
            }
        });
    }

    // 서버 퇴장 시, 서버 인원에 본인 제거
    public void OnExitServerCapacityUpdate(ServerData curServerData, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.name);

        reference.Child("curPlayerCount").RunTransaction(mutableData =>
        {
            if (mutableData.Value == null)
            {
                mutableData.Value = 1;
                return TransactionResult.Success(mutableData);
            }
            else if ((long)mutableData.Value <= 0)
            {
                long curUserCount = (long)mutableData.Value;
                mutableData.Value = 0;
                Debug.LogError("서버 인원 수가 음수로 나오려고 함. 우선 예외처리로 기능 작동에는 문제 없지만 확인 필요.");
                return TransactionResult.Success(mutableData);
            }
            else
            {
                long curUserCount = (long)mutableData.Value;
                mutableData.Value = curUserCount - 1;
                return TransactionResult.Success(mutableData);
            }
        });
    }

    // 서버 데이터로, 현재 접속 가능한지 여부 판단
    public void IsAbleToConnectServer(ServerData curServerData, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.name);

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
                int curPlayerCount = (int)snapshot.Child("curPlayerCount").Value;
                int maxPlayerCount = (int)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"서버 데이터 불러오기 성공. 현재 인원 / 최대 인원: {curPlayerCount}/{maxPlayerCount}");

                if(curPlayerCount < maxPlayerCount)
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

    // 서버 타입+이름으로 해당 서버 데이터 반환하기
    public void GetServerData(string name, ServerType type, Action<ServerData> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference serverRoot = GetServerBaseRef(type);
        DatabaseReference targetRef = serverRoot.Child(name);

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
    #endregion
}

[Serializable]
public class UserData
{
    public string name;
    public int level;
    public int money;
    public int kills;
    public float suvivalTime;
    public float highScore;
    public string startingPokemonName;

    public UserData(string name)
    {
        this.name = name;
        this.level = 1;
        this.money = 0;
        this.kills = 0;
        this.suvivalTime = 0;
        this.highScore = 0;
        this.startingPokemonName = "None";
    }
}

[Serializable]
public class ServerData
{
    public string name;
    public string id;
    public int type;
    public int maxPlayerCount;
    public int curPlayerCount;

    public ServerData() { }

    public ServerData(string name, string id, int maxPlayerCount)
    {
        this.name = name;
        this.id = id;
        this.maxPlayerCount = maxPlayerCount;
        curPlayerCount = 0;
    }
}
public enum ServerType { Lobby, InGame, TestServer, FunctionTestServer };

[Serializable]
public class LeaderBoardData
{
    [Serializable]
    public class Rank
    {

    }



}

