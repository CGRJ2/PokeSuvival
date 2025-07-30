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

        string roomName = "ItemBoxTest";

        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 4, IsVisible = true }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (PhotonNetwork.IsMasterClient)
        {
            InitializePool();
        }
        else
        {
            photonView.RPC(nameof(RequestActiveObjectsInfo), RpcTarget.MasterClient);
        }

    }

    [PunRPC]
    private void RequestActiveObjectsInfo()
    {
        if (PhotonNetwork.IsMasterClient)
        {
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
                    }
                }
            }

            if (activeViewIDs.Count > 0)
            {
                photonView.RPC(nameof(SyncActiveObjects), RpcTarget.Others, activeViewIDs.ToArray(), activePositions.ToArray());
            }
        }
    }

    [PunRPC]
    private void SyncActiveObjects(int[] activeViewIDs, Vector3[] positions)
    {
        StartCoroutine(ActivateObjectsAfterDelay(activeViewIDs, positions));
    }

    private IEnumerator ActivateObjectsAfterDelay(int[] activeViewIDs, Vector3[] positions)
    {
        yield return new WaitForSeconds(0.5f);

        int activatedCount = 0;

        for (int i = 0; i < activeViewIDs.Length; i++)
        {
            int viewID = activeViewIDs[i];
            Vector3 position = positions[i];

            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                pv.transform.position = position;
                pv.gameObject.SetActive(true);
                activatedCount++;
            }
        }
    }


    private GameObject CreateNewPooledObject()
    {
        if (pooledObjects.Count >= maxPoolSize)
        {
            Debug.LogWarning("최대 풀 크기에 도달");
            return null;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(itemBoxPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false);

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
        return null;
    }

    public void SpawnItemBox(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject itemBox = GetPooledObject();
            if (itemBox != null)
            {
                itemBox.transform.position = position;

                int viewID = itemBox.GetComponent<PhotonView>().ViewID;
                photonView.RPC(nameof(RPC_SetActiveObject), RpcTarget.AllBuffered, viewID, true, position);
            }
        }
    }

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
        }
    }

    public void DeactivateObject(GameObject obj)
    {
        if (obj.GetComponent<PhotonView>() != null)
        {
            int viewID = obj.GetComponent<PhotonView>().ViewID;
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
        }
    }



}