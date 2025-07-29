using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;


public class BackendManager : Singleton<BackendManager>
{
    public static FirebaseApp App { get; private set; }
    public static FirebaseAuth Auth { get; private set; }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("���̾� ���̽� ������ ��� �����Ǿ� ����� �� �ִ� ��Ȳ");
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError($"���̾� ���̽� ������ �������� �ʾ� �����߽��ϴ�. ����: {dependencyStatus}");
                App = null;
                Auth = null;
            }
        });
    }
}
