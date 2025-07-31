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

                if (_instance == null) { Debug.LogError($"�� �� �̱��� �ν��Ͻ� ��ü�� ����.\n(�ش� �α״� �׽�Ʈ ������ ���� ����ó����, �׽�Ʈ ���� �� �����ϰ� �����ϼŵ� �˴ϴ�)"); } 
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
