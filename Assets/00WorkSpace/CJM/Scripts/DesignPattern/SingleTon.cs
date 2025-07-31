using Photon.Pun;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null) { Debug.LogError($"씬 상에 싱글톤 인스턴스 객체가 없음.\n(해당 로그는 테스트 진행을 위한 예외처리로, 테스트 중일 땐 무시하고 진행하셔도 됩니다)"); } 
                else DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }

    protected void SingletonInit()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
    }
}

public class SingletonPUN<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }

    protected void SingletonInit()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
    }
}
