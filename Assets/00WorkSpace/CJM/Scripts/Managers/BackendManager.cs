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

    // �׽�Ʈ��
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            //LoadUserDataFromDB((data) => Debug.Log(data.name));
            Debug.Log(NetworkManager.Instance.CurServer.sceneName);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            //InitServerDataToServerInfoDB(new ServerData("Lobby Server 01 (KR)", "LobbyScene(CJM)", "�κ� 01", 0, "e4d01a07-2d0c-41bb-bc2d-59723abc27fc", 20));
            //InitServerDataToServerInfoDB(new ServerData("Lobby Server 02 (KR)", "LobbyScene(CJM)", "�κ� 02", 0, "4b17f092-1646-4668-9356-580cdb2e8529", 20));
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

    #region User Data ����

    // Auth - �α��� ���� ������ ������Ʈ
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
    public void InitUserDataToDB(UserData data, Action onSuccess = null, Action<string> onFail = null)
    {
        Debug.Log("DB�� UserData ù ����");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UserData ���� �۾��� ��ҵǾ����ϴ�.");
                onFail?.Invoke("�۾� ��ҵ�");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"UserData ���� ����: {task.Exception}");
                onFail?.Invoke("���� ����");
                return;
            }

            Debug.Log("UserData ���� ����");
            onSuccess?.Invoke();
        });
    }

    // DB - UserData ���� �� ����(UpdateChildrenAsync)
    public void UpdateUserDataValue(string key, object value)
    {
        Debug.Log($"UserData�� {key} ������ ����");

        string userId = Auth.CurrentUser.UserId;

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic[key] = value;

        userInfo.UpdateChildrenAsync(dic);
    }


    // DB - Auth.CurrentUser.UserId�� Ű������ ���� ������ �ҷ�����(GetValueAsync)
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

    #region Server Data ����

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
        DatabaseReference reference = GetServerBaseRef((ServerType)data.type).Child(data.key);

        ServerData serverInfo = new ServerData(data.key, data.sceneName, data.name, data.type, data.id, 20);
        string json = JsonUtility.ToJson(serverInfo);
        reference.SetRawJsonValueAsync(json);
    }

    // ���� ���� ��, ���� �ο��� ���� �߰�
    /*public void OnEnterServerCapacityUpdate(ServerData curServerData, List<string> memberIdList, Action onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.Child("curPlayerList").RunTransaction(mutableData =>
        {
            //Debug.LogWarning($"�� �̰� �ڲ� null�� ����? ������ ������ �ִµ�? {mutableData.Value}");
            if (mutableData.Value == null)
            {
                Debug.Log("curPlayerList�� ��� ���� ������");
                mutableData.Value = memberIdList.Cast<object>().ToList();
                return TransactionResult.Success(mutableData);
            }
            else
            {
                try
                {
                    // Firebase������ object ����Ʈ�� ��ȯ��
                    var existingList = new List<string>();
                    foreach (var item in (IEnumerable)mutableData.Value)
                    {
                        if (item != null)
                            existingList.Add(item.ToString());
                    }

                    List<string> curUserList = existingList.Select(o => o.ToString()).ToList();

                    Debug.Log("���� ���� ����Ʈ�� �߰���");
                    foreach (string id in memberIdList)
                    {
                        if (!curUserList.Contains(id))
                            curUserList.Add(id);
                        else Debug.LogError($"�̹� ������ �����ϴ� Id�� �� �߰��Ϸ�����: {id}");
                    }
                    
                    mutableData.Value = curUserList.Cast<object>().ToList(); // Firebase�� object ����Ʈ�� �����
                    return TransactionResult.Success(mutableData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"���� �ο� �߰� ���� - ����ȯ ����: {e.Message}");
                    return TransactionResult.Abort();
                }
            }
        });
    }*/

    // ���� ���� ��, ���� �ο��� ���� ����
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
                    // �ƹ� ����Ʈ�� ���ٸ� �� ����Ʈ�� �ʱ�ȭ
                    mutableData.Value = new List<object>();
                    onSucess?.Invoke();
                    return TransactionResult.Success(mutableData);
                }

                // Firebase������ object ����Ʈ�� ��ȯ��
                var existingList = new List<string>();
                foreach (var item in (IEnumerable)mutableData.Value)
                {
                    if (item != null)
                        existingList.Add(item.ToString());
                }
                List<string> curUserList = existingList.Select(o => o.ToString()).ToList();


                // ���� ���� ID ����
                if (curUserList.Contains(userId))
                    curUserList.Remove(userId);
                else Debug.LogError($"������ �����Ϸ��� Id�� �������� ����: {userId}");

                Debug.Log($"���� ���� ����Ʈ���� ���� �� �� ����Ʈ Count: {curUserList.Count}");


                // �ٽ� object ����Ʈ�� ����
                if (curUserList.Count < 1)
                    mutableData.Value = null;
                else
                    mutableData.Value = curUserList.Cast<object>().ToList();

                onSucess?.Invoke();
                return TransactionResult.Success(mutableData);
            }
            catch (Exception e)
            {
                Debug.LogError($"���� �ο� ���� ���� - ����ȯ ����: {e.Message}");
                onFail?.Invoke("�ο� ���� �� ���� �߻�");
                return TransactionResult.Abort();
            }
        });
    }*/

    // ���� �����ͷ�, ���� ���� �������� ���� �Ǵ�
    public void IsAbleToConnectServer(ServerData curServerData, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

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
                long curPlayerCount = (long)snapshot.Child("curPlayerCount").Value;
                long maxPlayerCount = (long)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"���� ������ �ҷ����� ����. ���� �ο� / �ִ� �ο�: {curPlayerCount}/{maxPlayerCount}");

                if (curPlayerCount < maxPlayerCount)
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

    public void CheckMultipleUsersSpaceAndReserve(ServerData curServerData, int multiUserCount, Action<bool> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);


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
                long curPlayerCount = (long)snapshot.Child("curPlayerCount").Value;
                long maxPlayerCount = (long)snapshot.Child("maxPlayerCount").Value;

                Debug.Log($"���� ������ �ҷ����� ����. ���� �ο� / �ִ� �ο�: {curPlayerCount}/{maxPlayerCount}");

                // �ڸ��� ������ �������ֱ�
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
                                Debug.LogError($"���� �ο� ���� ���� - ����ȯ ����: {e.Message}");
                                return TransactionResult.Abort();
                            }
                        }
                    }).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.LogError("�����Ϸ��� ������ �ڸ��� �ִµ� �ڸ� ���࿡ ������!");
                            onSuccess?.Invoke(false);
                        }
                        // �ڸ� ���� �Ϸ�Ǹ� ����
                        onSuccess?.Invoke(true);
                    });
                }
                // ������ false ��ȯ
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
    public void GetServerData(string key, ServerType type, Action<ServerData> onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference serverRoot = GetServerBaseRef(type);
        DatabaseReference targetRef = serverRoot.Child(key);

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
                //Debug.Log($"���� JSON: {json}");
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

    public void UpdateServerUserCount(ServerData curServerData, Action onSuccess = null, Action<string> onFail = null)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = GetServerBaseRef((ServerType)curServerData.type).Child(curServerData.key);

        reference.Child("curPlayerCount").RunTransaction(mutableData =>
        {
            //Debug.LogWarning($"�� �̰� �ڲ� null�� ����? ������ ������ �ִµ�? {mutableData.Value}");
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
                    Debug.LogError($"���� �ο� ���� ���� - ����ȯ ����: {e.Message}");
                    return TransactionResult.Abort();
                }
            }
        });
    }



    #endregion

    #region ��ŷ �ý���

    // ���� ��ŷ ������ ���� (�α��� or �Խ�Ʈ ���� => �׳� ������ ���� ���̵�, �г��Ӹ� ������ ��)
    public void InitLocalPlayerRankingData(RankData rankData)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(rankData.userId);

        string json = JsonUtility.ToJson(rankData);
        reference.SetRawJsonValueAsync(json);
    }

    // ���� ��ŷ ������ �ҷ����� (���� �񱳿�)
    public void LoadLocalPlayerRankData(string userId, Action<RankData> onSuccess = null, Action<string> onFail = null)
    {
        //Debug.LogWarning("��ŷ ������ �޾ƿ���");

        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(userId);

        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("��ŷ ������ �ҷ����� ��ҵ�.");
                onFail?.Invoke("��� �޽��� ���޿�");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"��ŷ ������ �ҷ����� ����: {task.Exception}");
                onFail?.Invoke("���� �޽��� ���޿�");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                RankData data = JsonUtility.FromJson<RankData>(json);
                Debug.Log($"��ŷ ������ �ҷ����� ����: {json}");
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("��ŷ �����Ͱ� �������� ����.");
                onFail?.Invoke("��ŷ �����Ͱ� ���ٴ� �޽��� ����");
            }
        });

    }

    // ���� �ű���� �ش� ������ ��ŷ �����Ϳ� ������Ʈ
    public void UpdateHighScore(int newScore, string UserId)
    {
        DatabaseReference root = Database.RootReference;
        DatabaseReference reference = root.Child("RankingBoardData").Child(UserId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        
        dic["name"] = PhotonNetwork.NickName; //�г����� �������� �� �����ϴ� �� ���
        dic["highScore"] = newScore;

        reference.UpdateChildrenAsync(dic);
    }

    // ��ŷ �����͸� �����ؼ� 1 ~ 10������ ��ȯ
    public void UpdateRankingBoard_SortedByScore(Action<List<KeyValuePair<string, RankData>>> onSuccess = null)
    {
        LoadAllRankData((dic) =>
        {
            // highScore ���� �������� ����
            var top10List = dic
                .OrderByDescending(kvp => kvp.Value.highScore)
                .Take(10)
                .ToList();

            //Debug.Log("��ŷ Top 10:");
            for (int i = 0; i < top10List.Count; i++)
            {
                var entry = top10List[i];
                //Debug.Log($"{i + 1}�� - {entry.Value.userName} / ����: {entry.Value.highScore}");
            }

            onSuccess?.Invoke(top10List);
        });
    }

    // ������ ���� �޾ƿ���
    public void GetRankNumb(string userId,Action<int> onSuccess = null, Action<string> onFail = null)
    {
        //Debug.LogWarning("��ũ ���� �޾ƿ���");
        LoadAllRankData((dic) =>
        {
            // highScore ���� �������� ����
            var sortedList = dic.OrderByDescending(kvp => kvp.Value.highScore).ToList();

            for (int i = 0; i < sortedList.Count; i++)
            {
                if (sortedList[i].Value.userId == userId)
                {
                    //Debug.LogWarning("�� ������ �߰�, ���� ��ȯ");
                    onSuccess?.Invoke(i);
                    return;
                }
            }
            Debug.LogWarning("�� ������ �߰� ����");
            onFail?.Invoke("��ŷ�� �� ���� ����");
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
                Debug.LogError("��ũ �����͵� �ҷ����� ����");
                onFail?.Invoke("�ҷ����� ����");
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

