using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonObjectPoolManager : MonoBehaviourPunCallbacks
{
    public static PhotonObjectPoolManager Instance { get; private set; }


    [SerializeField] private List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();
    private Dictionary<string, Pool> poolSettings = new();

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnJoinedRoom()
    {
        // if (PhotonObjectPoolManager.Instance != null)
        // {
        //     PhotonObjectPoolManager.Instance.InitializeAfterJoin();
        // }
        Debug.Log("[PoolManager] OnJoinedRoom 에서 풀 초기화 시도");
        InitializePools();
    }


    void Start()
    {
        GameObject test = PhotonNetwork.InstantiateRoomObject("Item", new Vector3(0, 0, 0), Quaternion.identity);
        if (test == null) Debug.LogError("테스트 프리팹 'Item' 을 InstantiateRoomObject로 찾을 수 없습니다.");
    }

    public bool HasKey(string key)
    {
        return poolDictionary.ContainsKey(key);
    }


    private void InitializePools()
    {
        if (!PhotonNetwork.IsMasterClient || isInitialized) return;
        isInitialized = true;

        foreach (var pool in pools)
        {
            var objectQueue = new Queue<GameObject>();
            poolDictionary[pool.key] = objectQueue;
            poolSettings[pool.key] = pool;

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewObject(pool);
                if (obj != null)
                {
                    obj.SetActive(false);
                    objectQueue.Enqueue(obj);
                }
            }
        }
    }

    private GameObject CreateNewObject(Pool pool)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PoolManager] 아직 룸에 입장하지 않아 InstantiateRoomObject를 수행할 수 없습니다.");
            return null;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(pool.prefab.name, new Vector3(9999, 9999, 9999), Quaternion.identity);
       // 임시 주석처리 obj.SetActive(false);
        return obj;
    }


    public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"[PoolManager] 풀 키 '{key}'가 등록되지 않았습니다.");
            return null;
        }

        var queue = poolDictionary[key];
        var pool = poolSettings[key];

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            obj = CreateNewObject(pool);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }
    public void ReturnToPool(string key, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] 풀 키 '{key}'가 없습니다: {key}");
            return;
        }

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }

}

[System.Serializable]
public class Pool
{
    public string key;              // 식별 키: "ItemBox", "ExpOrb", "Item"
    public GameObject prefab;       // 프리팹
    public int initialSize = 10;
    public int maxSize = 50;
}
