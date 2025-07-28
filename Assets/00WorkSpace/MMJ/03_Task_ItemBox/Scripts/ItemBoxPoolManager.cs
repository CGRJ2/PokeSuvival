using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemBoxPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject monsterBallPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 30; // �ִ� Ǯ ũ�� ����

    private List<GameObject> pooledObjects = new List<GameObject>();
    private static ItemBoxPoolManager instance;

    public static ItemBoxPoolManager Instance
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

    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPooledObject();
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // �� �̸� ����
        string roomName = "ItemBoxTest";

        // �ش� �̸��� ���� ������ �����ϰ�, ������ �����ϴ� ������� ����
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 4, IsVisible = true }, TypedLobby.Default);

        Debug.Log($"�� '{roomName}'�� ���� �õ� ��... �̹� �����ϸ� �����ϰ�, ������ �����մϴ�.");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Update ȣ��� / �������ΰ�? " + PhotonNetwork.IsMasterClient);
        // ������ Ŭ���̾�Ʈ�� ���� ���� ����

        // ������ Ŭ���̾�Ʈ�� �ʱ� Ǯ ����
        if (PhotonNetwork.IsMasterClient)
        {
            InitializePool();
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
        GameObject obj = PhotonNetwork.Instantiate("ItemBox", new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false);

        // ���̾��Ű ������ ���� �θ� ����
        obj.transform.SetParent(this.transform);

        ItemBox monsterBall = obj.GetComponent<ItemBox>();

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
    public void SpawnItemBox(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SpawnMonsterBall: RPC ȣ�� ����");
            photonView.RPC(nameof(RPC_SpawnItemBox), RpcTarget.AllBuffered, position);
        }
    }

    [PunRPC]
    private void RPC_SpawnItemBox(Vector3 position)
    {
        Debug.Log("RPC_SpawnItemBox ȣ���!");
        GameObject itemBox = GetPooledObject();
        if (itemBox != null)
        {
            Debug.Log("Ǯ���� ������Ʈ ������. ��ġ ���� �� Ȱ��ȭ.");
            itemBox.transform.position = position;
            itemBox.SetActive(true);
        }
        else
        {
            Debug.LogWarning("���ͺ��� ������ �� �����ϴ�. ��� ������ ������Ʈ�� �����ϴ�.");
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