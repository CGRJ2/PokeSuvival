using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemBoxPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject monsterBallPrefab;
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

    private void Start()
    {
        // 마스터 클라이언트만 초기 풀 생성
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
            Debug.LogWarning("최대 풀 크기에 도달");
            return null;
        }

        // [[버그수정을 위한 주석처리]] GameObject obj = PhotonNetwork.InstantiateRoomObject(monsterBallPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        GameObject obj = PhotonNetwork.Instantiate("ItemBox", new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false);

        // 하이어라키 정리를 위해 부모 설정
        obj.transform.SetParent(this.transform);

        ItemBox monsterBall = obj.GetComponent<ItemBox>();

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

        // 모든 오브젝트가 사용 중이면 새로 생성하지 않음
        Debug.Log("사용 가능한 오브젝트가 없음");
        return null;
    }

    // 마스터 클라이언트에서 몬스터볼 스폰 요청
    public void SpawnMonsterBall(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SpawnMonsterBall: RPC 호출 시작");
            photonView.RPC(nameof(RPC_SpawnMonsterBall), RpcTarget.AllBuffered, position);
        }
    }

    [PunRPC]
    private void RPC_SpawnMonsterBall(int viewID, Vector3 position)
    {
        Debug.Log("RPC_SpawnMonsterBall 호출됨!");
        GameObject monsterBall = GetPooledObject();
        if (monsterBall != null)
        {
            Debug.Log("풀에서 오브젝트 가져옴. 위치 지정 후 활성화.");
            monsterBall.transform.position = position;
            monsterBall.SetActive(true);
        }
        else
        {
            Debug.LogWarning("몬스터볼을 스폰할 수 없습니다. 사용 가능한 오브젝트가 없습니다.");
        }
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            GameObject obj = pv.gameObject;
            obj.transform.position = position;
            obj.SetActive(true);
        }
    }

    // 몬스터볼 파괴 (풀로 반환)
    public void ReturnToPool(GameObject obj)
    {
        if (PhotonNetwork.IsMasterClient) // 방장만
        {
            photonView.RPC(nameof(RPC_ReturnToPool), RpcTarget.AllBuffered, obj.GetComponent<PhotonView>().ViewID); // 도로 풀에 넣으라고 모두 전달
        }
    }

    [PunRPC]
    private void RPC_ReturnToPool(int viewID) //뷰 아이디에 해당하는 오브젝트 비활성화
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            pv.gameObject.SetActive(false);
        }
    }
}