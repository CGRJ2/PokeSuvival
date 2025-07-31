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

    // �׽�Ʈ��
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
                Debug.Log("���̾� ���̽� ������ ��� �����Ǿ� ����� �� �ִ� ��Ȳ");
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;
            }
            else
            {
                Debug.LogError($"���̾� ���̽� ������ �������� �ʾ� �����߽��ϴ�. ����: {dependencyStatus}");
                App = null;
                Auth = null;
                Database = null;
            }
        });
    }

    // DB�� ������ ���� ���� �Ǵ�
    public void CheckData(DatabaseReference dbRef, Action<bool> onChecked = null)
    {
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("������ ���� ���� Ȯ�� ����");
                onChecked?.Invoke(false); // ������ false ó��
                return;
            }

            DataSnapshot snapshot = task.Result;
            bool exists = snapshot.Exists;

            Debug.Log($"������ ���� ����: {exists}");
            onChecked?.Invoke(exists);
        });
    }

    #region User Data

    // Auth - ���� ������ ������Ʈ
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
                    Debug.LogError("���� �г��� ���� ��ҵ�.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"���� �г��� ���� ������. ���л���: {task.Exception}, ErrorCode: {((FirebaseException)task.Exception.InnerException).ErrorCode}");

                    return;
                }

                Debug.Log("���� �������� ���������� ������Ʈ ��");
            });
        }
    }

    // DB - UserData ����(SetRawJsonValueAsync)
    public void InitUserDataToDB(UserData data)
    {
        Debug.Log("DB�� UserData ù ����");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json);
    }

    // DB - UserData ���� �� ����(UpdateChildrenAsync)
    public void UpdateUserData(string key, object value)
    {
        Debug.Log($"UserData�� {key} ������ ����");

        string userId = Auth.CurrentUser.UserId;

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic[key] = value;

        userInfo.UpdateChildrenAsync(dic);
    }

    // DB - ���� ������ �ҷ�����(GetValueAsync)
    public void LoadUserDataFromDB(Action<UserData> onSuccess = null, Action<string> onFail = null)
    {
        string userId = Auth.CurrentUser.UserId;
        DatabaseReference userInfo = Database.RootReference.Child("UserData").Child(userId);

        userInfo.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("���� ������ �ҷ����� ��ҵ�.");
                onFail?.Invoke("��� �޽��� ���޿�");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"���� ������ �ҷ����� ����: {task.Exception}");
                onFail?.Invoke("���� �޽��� ���޿�");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                UserData data = JsonUtility.FromJson<UserData>(json);
                Debug.Log($"���� ������ �ҷ����� ����: {json}");
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("���� �����Ͱ� �������� ����.");
                onFail?.Invoke("���� �����Ͱ� ���ٴ� �޽��� ����");
            }
        });

    }

    #endregion

    #region Server State Data

    // ���� ������ DatabaseReference Root ����
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

    // DB - Server Data ����(SetRawJsonValueAsync)
    public void InitServerDataToServerInfoDB(ServerData data)
    {
        Debug.Log("DB�� �ش� Server Data�� Server Info ù ����");

        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)data.type).Child(data.name);

        ServerData serverInfo = new ServerData(data.name, data.id, 20);
        string json = JsonUtility.ToJson(serverInfo);
        reference.SetRawJsonValueAsync(json);
    }

    // ���� ���� ��, ���� �ο��� ���� �߰�
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

    // ���� ���� ��, ���� �ο��� ���� ����
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
                Debug.LogError("���� �ο� ���� ������ �������� ��. �켱 ����ó���� ��� �۵����� ���� ������ Ȯ�� �ʿ�.");
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

    // ���� �����ͷ�, ���� ���� �������� ���� �Ǵ�
    public void IsAbleToConnectServer(ServerData curServerData, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.name);

        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("���� ������ �ҷ����� ��ҵ�.");
                onFail?.Invoke("��� �޽��� ���޿�");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"���� ������ �ҷ����� ����: {task.Exception}");
                onFail?.Invoke("���� �޽��� ���޿�");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                int curPlayerCount = (int)snapshot.Child("curPlayerCount").Value;
                int maxPlayerCount = (int)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"���� ������ �ҷ����� ����. ���� �ο� / �ִ� �ο�: {curPlayerCount}/{maxPlayerCount}");

                if(curPlayerCount < maxPlayerCount)
                    onSuccess?.Invoke(true);
                else 
                    onSuccess?.Invoke(false);
            }
            else
            {
                Debug.LogWarning("�ش� ������ �����ͺ��̽��� �������� ����.");
                onFail?.Invoke("�ش� ������ �����ͺ��̽��� ���ٴ� �޽��� ����");
            }
        });
    }

    // ���� Ÿ��+�̸����� �ش� ���� ������ ��ȯ�ϱ�
    public void GetServerData(string name, ServerType type, Action<ServerData> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference serverRoot = GetServerBaseRef(type);
        DatabaseReference targetRef = serverRoot.Child(name);

        targetRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("���� �ҷ����� ����");
                onFail?.Invoke("�ҷ����� ����");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                ServerData data = JsonUtility.FromJson<ServerData>(json);
                Debug.Log($"���� ������ �ҷ����� ����: {json}");

                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("���� �����Ͱ� �������� ����.");
                onFail?.Invoke("���� �����Ͱ� ���ٴ� �޽��� ����");
            }

        });
    }

    // �ش� ������ ��� ���� ��ȯ (ex. type = InGameServer ��� ��� �ΰ��� ������ ��ųʸ��� ������)
    public void LoadAllTargetTypeServers(ServerType type, Action<Dictionary<string, ServerData>> onSuccess = null, Action<string> onFail = null)
    {
        // �ش� Ÿ���� ����� ����
        DatabaseReference serverRoot = GetServerBaseRef(type);
        Debug.Log($"������ �ҷ�����... Ÿ��:{type}");
        serverRoot.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("���� ����Ʈ �ҷ����� ����");
                onFail?.Invoke("�ҷ����� ����");
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

    // ���� ����Ʈ �߿� 1. ���� �ο��� ����, 2. ���� ������ ������ ��ȯ
    public void QuickSearchAccessableServer(Dictionary<string, ServerData> serverDic, Action<ServerData> onSuccess, Action<string> onFail = null)
    {
        Debug.Log("����Ʈ ���� ã��");


        if (serverDic == null || serverDic.Count == 0)
        {
            onFail?.Invoke("���� ��ųʸ��� ��� ����");
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
            Debug.Log($"QuickSearch ���: {bestServer.name} (���� ���� �ο�: {bestServer.curPlayerCount}/{bestServer.maxPlayerCount})");
            onSuccess?.Invoke(bestServer);
        }
        else
        {
            Debug.LogWarning("���� ������ ������ ����");
            onFail?.Invoke("���� ������ ���� ����");
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

