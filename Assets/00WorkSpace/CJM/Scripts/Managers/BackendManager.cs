using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;


public class BackendManager : Singleton<BackendManager>
{
    public static FirebaseApp App { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }


    //[SerializeField] string testText;
    [SerializeField] public UserData test;

    public void testBtn()
    {
        InitUserDataToDB(new UserData(PhotonNetwork.LocalPlayer.NickName));
        /*Debug.Log("��ư ����");
        DatabaseReference reference = Database.RootReference;
        reference.SetValueAsync(testText);*/
    }
    
   

    void Start()
    {
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

    // Auth ���� ������ ������Ʈ
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


    // DB�� UserData ����
    public void InitUserDataToDB(UserData data)
    {
        Debug.Log("DB�� UserData ù ����");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json);
    }

    // UserData ���� �� ����
    public void UpdateUserData(string key, object value)
    {
        Debug.Log($"UserDatas�� {key} ������ ����");

        string userId = Auth.CurrentUser.UserId;

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic[key] = value;

        userInfo.UpdateChildrenAsync(dic);
    }

    public void LoadUserDataFromDB(Action<UserData> onSuccess = null, Action<string> onFail = null)
    {
        string userId = Auth.CurrentUser.UserId;
        DatabaseReference userInfo = Database.RootReference.Child("UserDatas").Child(userId);

        userInfo.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("���� ������ �ҷ����� ��ҵ�.");
                onFail?.Invoke("��ҵ�");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"���� ������ �ҷ����� ����: {task.Exception}");
                onFail?.Invoke("����");
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
                onFail?.Invoke("�������� ����");
            }
        });
    }
}

[Serializable]
public class UserData
{
    public string name;
    public int level;
    public int money;
    public float playTime;
    public string startingPokemonName;

    public UserData(string name)
    {
        this.name = name;
        this.level = 1;
        this.money = 0;
        this.playTime = 0;
        this.startingPokemonName = "None";
    }
}
