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
        /*Debug.Log("버튼 누름");
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

    // Auth 유저 프로필 업데이트
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


    // DB에 UserData 생성
    public void InitUserDataToDB(UserData data)
    {
        Debug.Log("DB에 UserData 첫 생성");

        string userId = Auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);

        DatabaseReference root = Database.RootReference;
        DatabaseReference userInfo = root.Child("UserData").Child(userId);

        userInfo.SetRawJsonValueAsync(json);
    }

    // UserData 내부 값 변경
    public void UpdateUserData(string key, object value)
    {
        Debug.Log($"UserDatas의 {key} 데이터 변경");

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
                Debug.LogError("유저 데이터 불러오기 취소됨.");
                onFail?.Invoke("취소됨");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"유저 데이터 불러오기 실패: {task.Exception}");
                onFail?.Invoke("실패");
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
                onFail?.Invoke("존재하지 않음");
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
