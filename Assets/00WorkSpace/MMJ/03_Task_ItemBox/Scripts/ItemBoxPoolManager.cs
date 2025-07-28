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

        // 방 이름 설정
        string roomName = "ItemBoxTest";

        // 해당 이름의 방이 있으면 참가하고, 없으면 생성하는 방식으로 수정
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 4, IsVisible = true }, TypedLobby.Default);

        Debug.Log($"방 '{roomName}'에 참가 시도 중... 이미 존재하면 참가하고, 없으면 생성합니다.");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Update 호출됨 / 마스터인가? " + PhotonNetwork.IsMasterClient);

        if (PhotonNetwork.IsMasterClient)
        {
            InitializePool();
        }
        else
        {
            // 마스터가 아닌 클라이언트는 현재 활성화된 오브젝트 정보를 요청
            Debug.Log("마스터 클라이언트에게 활성화된 오브젝트 정보 요청");
            photonView.RPC(nameof(RequestActiveObjectsInfo), RpcTarget.MasterClient);
        }

    }

    [PunRPC]
    private void RequestActiveObjectsInfo()
    {
        // 마스터 클라이언트만 응답
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("활성화된 오브젝트 정보 요청 받음. 정보 수집 시작...");

            // 현재 활성화된 모든 오브젝트의 ViewID를 수집
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
                        Debug.Log($"활성화된 오브젝트 발견: ViewID {pv.ViewID}, 위치: {obj.transform.position}");
                    }
                }
            }

            // 늦게 참가한 클라이언트에게 활성화된 오브젝트 정보 전송
            if (activeViewIDs.Count > 0)
            {
                Debug.Log($"활성화된 오브젝트 {activeViewIDs.Count}개 정보 전송");
                photonView.RPC(nameof(SyncActiveObjects), RpcTarget.Others, activeViewIDs.ToArray(), activePositions.ToArray());
            }
            else
            {
                Debug.Log("활성화된 오브젝트가 없습니다.");
            }
        }
    }

    [PunRPC]
    private void SyncActiveObjects(int[] activeViewIDs, Vector3[] positions)
    {
        Debug.Log($"마스터 클라이언트로부터 {activeViewIDs.Length}개의 활성화된 오브젝트 정보 수신");

        // 약간의 지연 후 오브젝트 활성화 (모든 오브젝트가 생성될 시간을 주기 위함)
        StartCoroutine(ActivateObjectsAfterDelay(activeViewIDs, positions));
    }

    private IEnumerator ActivateObjectsAfterDelay(int[] activeViewIDs, Vector3[] positions)
    {
        Debug.Log("오브젝트 활성화 준비 중... 잠시 대기");

        // 모든 오브젝트가 생성될 때까지 약간 대기
        // 이 시간은 게임의 복잡성과 오브젝트 수에 따라 조정 필요
        yield return new WaitForSeconds(0.5f);

        int activatedCount = 0;

        for (int i = 0; i < activeViewIDs.Length; i++)
        {
            int viewID = activeViewIDs[i];
            Vector3 position = positions[i];

            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                // 위치 설정 및 활성화
                pv.transform.position = position;
                pv.gameObject.SetActive(true);
                activatedCount++;

                Debug.Log($"오브젝트 활성화 성공: ViewID {viewID}, 위치: {position}");
            }
            else
            {
                Debug.LogWarning($"ViewID {viewID}를 가진 PhotonView를 찾을 수 없습니다.");
            }
        }

        Debug.Log($"늦게 참가: 총 {activatedCount}개의 오브젝트 활성화 완료");
    }


    private GameObject CreateNewPooledObject()
    {
        if (pooledObjects.Count >= maxPoolSize)
        {
            Debug.LogWarning("최대 풀 크기에 도달");
            return null;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(itemBoxPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false); // 로컬에서만 비활성화됨

        // 하이어라키 정리를 위해 부모 설정 (로컬에서만 적용됨)
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

        Debug.Log("사용 가능한 오브젝트가 없음");
        return null;
    }

    // 마스터 클라이언트에서 몬스터볼 스폰 요청
    public void SpawnItemBox(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject itemBox = GetPooledObject();
            if (itemBox != null)
            {
                // 위치 설정
                itemBox.transform.position = position;

                // 모든 클라이언트에게 활성화 요청 (ViewID로 식별)
                int viewID = itemBox.GetComponent<PhotonView>().ViewID;
                photonView.RPC(nameof(RPC_SetActiveObject), RpcTarget.AllBuffered, viewID, true, position);
            }
            else
            {
                Debug.LogWarning("몬스터볼을 스폰할 수 없습니다. 사용 가능한 오브젝트가 없습니다.");
            }
        }
    }

    // 몬스터볼 파괴 (풀로 반환)
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
                pv.transform.position = position; // 위치 설정
            }
            pv.gameObject.SetActive(active);
            Debug.Log($"오브젝트 {viewID}를 {(active ? "활성화" : "비활성화")}했습니다.");
        }
        else
        {
            Debug.LogError($"ViewID {viewID}를 가진 PhotonView를 찾을 수 없습니다.");
        }
    }

    public void DeactivateObject(GameObject obj)
    {
        if (obj.GetComponent<PhotonView>() != null)
        {
            int viewID = obj.GetComponent<PhotonView>().ViewID;
            // 모든 클라이언트에게 비활성화 요청 전송 (버퍼링 포함)
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
            Debug.Log($"오브젝트 {viewID}가 비활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning($"ViewID {viewID}를 가진 PhotonView를 찾을 수 없습니다.");
        }
    }



}