using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterBallPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject monsterBallPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 30; // 최대 풀 크기 제한

    private List<GameObject> pooledObjects = new List<GameObject>();
    private static MonsterBallPoolManager instance;

    public static MonsterBallPoolManager Instance
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

        GameObject obj = PhotonNetwork.InstantiateRoomObject(monsterBallPrefab.name, new Vector3(0, -100, 0), Quaternion.identity);
        obj.SetActive(false);

        // 하이어라키 정리를 위해 부모 설정
        obj.transform.SetParent(this.transform);

        MonsterBall monsterBall = obj.GetComponent<MonsterBall>();

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
            photonView.RPC(nameof(RPC_SpawnMonsterBall), RpcTarget.AllBuffered, position);
        }
    }

    [PunRPC]
    private void RPC_SpawnMonsterBall(Vector3 position)
    {
        GameObject monsterBall = GetPooledObject();
        if (monsterBall != null)
        {
            monsterBall.transform.position = position;
            monsterBall.SetActive(true);
        }
        else
        {
            Debug.LogWarning("몬스터볼을 스폰할 수 없습니다. 사용 가능한 오브젝트가 없습니다.");
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