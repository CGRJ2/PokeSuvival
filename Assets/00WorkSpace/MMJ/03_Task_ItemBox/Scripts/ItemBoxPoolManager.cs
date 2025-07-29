using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class ItemBoxPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject itemBoxPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 30; // 최대 풀 크기 제한

    private List<GameObject> pooledObjects = new List<GameObject>();
    private static ItemBoxPoolManager instance;

    public static ItemBoxPoolManager Instance
    {
        get { return instance; }
    }

    private void Awake() //싱글톤
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
        if (PhotonNetwork.IsMasterClient)
        {
            InitializePool();
        }
        else
        {
            // �����Ͱ� �ƴ� Ŭ���̾�Ʈ�� ���� Ȱ��ȭ�� ������Ʈ ������ ��û
            Debug.Log("������ Ŭ���̾�Ʈ���� Ȱ��ȭ�� ������Ʈ ���� ��û");
            photonView.RPC(nameof(RequestActiveObjectsInfo), RpcTarget.MasterClient);
        }

    }

    [PunRPC]
    private void RequestActiveObjectsInfo()
    {
        // ������ Ŭ���̾�Ʈ�� ����
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Ȱ��ȭ�� ������Ʈ ���� ��û ����. ���� ���� ����...");

            // ���� Ȱ��ȭ�� ��� ������Ʈ�� ViewID�� ����
            List<int> activeViewIDs = new List<int>();
            List<Vector3> activePositions = new List<Vector3>();

            foreach (GameObject obj in pooledObjects)
            {
                if (obj.activeInHierarchy)
                {
                    PhotonView pv = obj.GetComponent<PhotonView>();
                    if (pv != null)
                    {
                        activeViewIDs.Add(pv.ViewID);
                        activePositions.Add(obj.transform.position);
                        Debug.Log($"Ȱ��ȭ�� ������Ʈ �߰�: ViewID {pv.ViewID}, ��ġ: {obj.transform.position}");
                    }
                }
            }

            // �ʰ� ������ Ŭ���̾�Ʈ���� Ȱ��ȭ�� ������Ʈ ���� ����
            if (activeViewIDs.Count > 0)
            {
                Debug.Log($"Ȱ��ȭ�� ������Ʈ {activeViewIDs.Count}�� ���� ����");
                photonView.RPC(nameof(SyncActiveObjects), RpcTarget.Others, activeViewIDs.ToArray(), activePositions.ToArray());
            }
            else
            {
                Debug.Log("Ȱ��ȭ�� ������Ʈ�� �����ϴ�.");
            }
        }
    }

    [PunRPC]
    private void SyncActiveObjects(int[] activeViewIDs, Vector3[] positions)
    {
        Debug.Log($"������ Ŭ���̾�Ʈ�κ��� {activeViewIDs.Length}���� Ȱ��ȭ�� ������Ʈ ���� ����");

        // �ణ�� ���� �� ������Ʈ Ȱ��ȭ (��� ������Ʈ�� ������ �ð��� �ֱ� ����)
        StartCoroutine(ActivateObjectsAfterDelay(activeViewIDs, positions));
    }

    private IEnumerator ActivateObjectsAfterDelay(int[] activeViewIDs, Vector3[] positions)
    {
        Debug.Log("������Ʈ Ȱ��ȭ �غ� ��... ��� ���");

        // ��� ������Ʈ�� ������ ������ �ణ ���
        // �� �ð��� ������ ���⼺�� ������Ʈ ���� ���� ���� �ʿ�
        yield return new WaitForSeconds(0.5f);

        int activatedCount = 0;

        for (int i = 0; i < activeViewIDs.Length; i++)
        {
            int viewID = activeViewIDs[i];
            Vector3 position = positions[i];

            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                // ��ġ ���� �� Ȱ��ȭ
                pv.transform.position = position;
                pv.gameObject.SetActive(true);
                activatedCount++;

                Debug.Log($"������Ʈ Ȱ��ȭ ����: ViewID {viewID}, ��ġ: {position}");
            }
            else
            {
                Debug.LogWarning($"ViewID {viewID}�� ���� PhotonView�� ã�� �� �����ϴ�.");
            }
        }

        Debug.Log($"�ʰ� ����: �� {activatedCount}���� ������Ʈ Ȱ��ȭ �Ϸ�");
    }


    private GameObject CreateNewPooledObject()
    {
        if (pooledObjects.Count >= maxPoolSize)
        {
            Debug.LogWarning("최대 풀 크기에 도달");
            return null;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(itemBoxPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false); // ���ÿ����� ��Ȱ��ȭ��

        // ���̾��Ű ������ ���� �θ� ���� (���ÿ����� �����)
        obj.transform.SetParent(this.transform);

        pooledObjects.Add(obj);
        return obj;
    }

    public GameObject GetPooledObject()
    {
        // 비활성화된 오브젝트 찾기
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        Debug.Log("��� ������ ������Ʈ�� ����");
        return null;
    }

    // ������ Ŭ���̾�Ʈ���� ���ͺ� ���� ��û
    public void SpawnItemBox(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject itemBox = GetPooledObject();
            if (itemBox != null)
            {
                // ��ġ ����
                itemBox.transform.position = position;

                // ��� Ŭ���̾�Ʈ���� Ȱ��ȭ ��û (ViewID�� �ĺ�)
                int viewID = itemBox.GetComponent<PhotonView>().ViewID;
                photonView.RPC(nameof(RPC_SetActiveObject), RpcTarget.AllBuffered, viewID, true, position);
            }
            else
            {
                Debug.LogWarning("���ͺ��� ������ �� �����ϴ�. ��� ������ ������Ʈ�� �����ϴ�.");
            }
        }
    }

    // ���ͺ� �ı� (Ǯ�� ��ȯ)
    public void ReturnToPool(GameObject obj)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int viewID = obj.GetComponent<PhotonView>().ViewID;
            photonView.RPC(nameof(RPC_SetActiveObject), RpcTarget.AllBuffered, viewID, false, Vector3.zero);
        }
    }

    [PunRPC]
    private void RPC_SetActiveObject(int viewID, bool active, Vector3 position)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            if (active)
            {
                pv.transform.position = position; // ��ġ ����
            }
            pv.gameObject.SetActive(active);
            Debug.Log($"������Ʈ {viewID}�� {(active ? "Ȱ��ȭ" : "��Ȱ��ȭ")}�߽��ϴ�.");
        }
        else
        {
            Debug.LogError($"ViewID {viewID}�� ���� PhotonView�� ã�� �� �����ϴ�.");
        }
    }

    public void DeactivateObject(GameObject obj)
    {
        if (obj.GetComponent<PhotonView>() != null)
        {
            int viewID = obj.GetComponent<PhotonView>().ViewID;
            // ��� Ŭ���̾�Ʈ���� ��Ȱ��ȭ ��û ���� (���۸� ����)
            photonView.RPC(nameof(RPC_DeactivateObject), RpcTarget.AllBuffered, viewID);
        }
    }

    [PunRPC]
    private void RPC_DeactivateObject(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            pv.gameObject.SetActive(false);
            Debug.Log($"������Ʈ {viewID}�� ��Ȱ��ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning($"ViewID {viewID}�� ���� PhotonView�� ã�� �� �����ϴ�.");
        }
    }



}