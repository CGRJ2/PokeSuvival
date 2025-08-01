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
        Debug.Log("[PoolManager] OnJoinedRoom ���� Ǯ �ʱ�ȭ �õ�");
        InitializePools();
    }


    void Start()
    {
        GameObject test = PhotonNetwork.InstantiateRoomObject("Item", new Vector3(0, 0, 0), Quaternion.identity);
        if (test == null) Debug.LogError("�׽�Ʈ ������ 'Item' �� InstantiateRoomObject�� ã�� �� �����ϴ�.");
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
            Debug.LogWarning("[PoolManager] ���� �뿡 �������� �ʾ� InstantiateRoomObject�� ������ �� �����ϴ�.");
            return null;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(pool.prefab.name, new Vector3(9999, 9999, 9999), Quaternion.identity);
       // �ӽ� �ּ�ó�� obj.SetActive(false);
        return obj;
    }


    public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"[PoolManager] Ǯ Ű '{key}'�� ��ϵ��� �ʾҽ��ϴ�.");
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
            Debug.LogWarning($"[PoolManager] Ǯ Ű '{key}'�� �����ϴ�: {key}");
            return;
        }

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }

}

[System.Serializable]
public class Pool
{
    public string key;              // �ĺ� Ű: "ItemBox", "ExpOrb", "Item"
    public GameObject prefab;       // ������
    public int initialSize = 10;
    public int maxSize = 50;
}
