using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterBallPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject monsterBallPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 30; // �ִ� Ǯ ũ�� ����

    private List<GameObject> pooledObjects = new List<GameObject>();
    private static MonsterBallPoolManager instance;

    public static MonsterBallPoolManager Instance
    {
        get { return instance; }
    }

    private void Awake() //�̱���
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ������ Ŭ���̾�Ʈ�� �ʱ� Ǯ ����
        if (PhotonNetwork.IsMasterClient)
        {
            InitializePool();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPooledObject();
        }
    }

    private GameObject CreateNewPooledObject()
    {
        if (pooledObjects.Count >= maxPoolSize)
        {
            Debug.LogWarning("�ִ� Ǯ ũ�⿡ ����");
            return null;
        }

        // [[���׼����� ���� �ּ�ó��]] GameObject obj = PhotonNetwork.InstantiateRoomObject(monsterBallPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        GameObject obj = PhotonNetwork.Instantiate("MonsterBall", new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false);

        // ���̾��Ű ������ ���� �θ� ����
        obj.transform.SetParent(this.transform);

        MonsterBall monsterBall = obj.GetComponent<MonsterBall>();

        pooledObjects.Add(obj);
        return obj;
    }

    public GameObject GetPooledObject()
    {
        // ��Ȱ��ȭ�� ������Ʈ ã��
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        // ��� ������Ʈ�� ��� ���̸� ���� �������� ����
        Debug.Log("��� ������ ������Ʈ�� ����");
        return null;
    }

    // ������ Ŭ���̾�Ʈ���� ���ͺ� ���� ��û
    public void SpawnMonsterBall(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SpawnMonsterBall: RPC ȣ�� ����");
            photonView.RPC(nameof(RPC_SpawnMonsterBall), RpcTarget.AllBuffered, position);
        }
    }

    [PunRPC]
    private void RPC_SpawnMonsterBall(int viewID, Vector3 position)
    {
        Debug.Log("RPC_SpawnMonsterBall ȣ���!");
        GameObject monsterBall = GetPooledObject();
        if (monsterBall != null)
        {
            Debug.Log("Ǯ���� ������Ʈ ������. ��ġ ���� �� Ȱ��ȭ.");
            monsterBall.transform.position = position;
            monsterBall.SetActive(true);
        }
        else
        {
            Debug.LogWarning("���ͺ��� ������ �� �����ϴ�. ��� ������ ������Ʈ�� �����ϴ�.");
        }
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            GameObject obj = pv.gameObject;
            obj.transform.position = position;
            obj.SetActive(true);
        }
    }

    // ���ͺ� �ı� (Ǯ�� ��ȯ)
    public void ReturnToPool(GameObject obj)
    {
        if (PhotonNetwork.IsMasterClient) // ���常
        {
            photonView.RPC(nameof(RPC_ReturnToPool), RpcTarget.AllBuffered, obj.GetComponent<PhotonView>().ViewID); // ���� Ǯ�� ������� ��� ����
        }
    }

    [PunRPC]
    private void RPC_ReturnToPool(int viewID) //�� ���̵� �ش��ϴ� ������Ʈ ��Ȱ��ȭ
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            pv.gameObject.SetActive(false);
        }
    }
}