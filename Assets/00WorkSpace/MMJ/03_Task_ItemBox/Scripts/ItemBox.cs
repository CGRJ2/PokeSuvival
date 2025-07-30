using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Runtime.Serialization;


public class ItemBox : MonoBehaviourPun, IPunObservable, IDamagable
{
    [Header("아이템 설정(드롭 확률 변수는 비율입니다.)")]
    [SerializeField] private GameObject[] itemPrefabs; // 여러 아이템 프리팹 배열로 변경
    [SerializeField] private float[] dropProbabilities; // 각 아이템의 드롭 확률

    [Header("배틀 데이터")]

    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;

    // IDamagable 인터페이스 구현
    public BattleDataTable BattleData
    {
        get => new BattleDataTable(
            -1,
            null,
            default,
            maxHp,
            currentHp);
    }

    // 데미지를 받는 메서드 구현
    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        //if (!photonView.IsMine && PhotonNetwork.IsConnected)
        //{
        //    Debug.Log("아이템박스 내꺼 아님");
        //    return false;
        //}

        // 스킬과 공격 데이터를 기반으로 데미지 계산
        int damage = 4;
        currentHp -= damage; // TODO : 나중에는 랜덤으로
		PlayerManager.Instance?.ShowDamageText(transform, damage, Color.white);

        photonView.RPC(nameof(RPC_SyncHp), RpcTarget.OthersBuffered, currentHp);

        // 체력이 0 이하면 파괴
        if (currentHp <= 0)
        {
            Debug.Log("아이템박스 부셔짐 방장 호출");
            photonView.RPC(nameof(RPC_OnHit), RpcTarget.MasterClient);
            return false;
		}

        return true;
    }

    [PunRPC]
    public void RPC_SyncHp(int _currentHp)
    {
        currentHp = _currentHp;
    }

    // 플레이어 공격에 의해 파괴될 때 호출
    [PunRPC]
    public void RPC_OnHit()
    {
        Debug.Log("방장이 아이템박스가 부셔졌을 떄 아이템 랜덤 드롭");
        if (PhotonNetwork.IsMasterClient)
        {
            // 몬스터볼의 현재 위치 저장
            Vector3 spawnPosition = transform.position;

            // 랜덤 아이템 선택 후 드롭
            int selectedItemIndex = GetRandomItemIndex();

            // 선택된 아이템 인덱스를 RPC로 전달
            photonView.RPC(nameof(RPC_DropItem), RpcTarget.MasterClient, spawnPosition, selectedItemIndex);

            // 풀로 반환
            ItemBoxPoolManager.Instance.ReturnToPool(gameObject);
        }
    }

    // 확률에 따라 랜덤 아이템 인덱스 선택
    private int GetRandomItemIndex()
    {
        // 아이템이나 확률 배열이 없거나 길이가 다르면 -1 반환
        if (itemPrefabs == null || dropProbabilities == null ||
            itemPrefabs.Length == 0 || itemPrefabs.Length != dropProbabilities.Length)
        {
            Debug.LogWarning("아이템 프리팹 또는 확률 설정이 올바르지 않습니다.");
            return -1;
        }

        // 확률 합계 계산 (합이 1이 아닐 수 있으므로)
        float totalProbability = 0f;
        foreach (float prob in dropProbabilities)
        {
            totalProbability += prob;
        }

        // 0~1 사이의 랜덤 값 생성
        float randomValue = Random.value * totalProbability;

        // 확률에 따라 아이템 선택
        float cumulativeProbability = 0f;

        for (int i = 0; i < dropProbabilities.Length; i++)
        {
            cumulativeProbability += dropProbabilities[i];

            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        // 기본값으로 첫 번째 아이템 반환
        return 0;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내 데이터를 보냄
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(currentHp);
        }
        else
        {
            // 상대방의 데이터를 받음
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            currentHp = (int)stream.ReceiveNext();
        }
    }


    //private void OnTriggerEnter2D(Collider2D collider) // 테스트용 코드
    //{
    //    // 플레이어에 닿으면 아이템이 먹힘
    //    if (collider.CompareTag("Player"))
    //    {
    //        // 자기 자신(아이템)을 비활성화
    //        ItemBoxPoolManager.Instance.DeactivateObject(this.gameObject);
    //    }
    //}

    [PunRPC]
    private void RPC_DropItem(Vector3 position, int itemIndex)
    {
        // 아이템 인덱스가 유효하지 않으면 리턴
        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
        {
            Debug.LogWarning("유효하지 않은 아이템 인덱스입니다: " + itemIndex);
            return;
        }

        // 선택된 아이템 프리팹 생성
        Debug.Log($"{itemIndex}번째 아이템[{itemPrefabs[itemIndex].name}] 생성 시도");
        //string prefabPath = "Items/" + itemPrefabs[itemIndex].name; // Resources 폴더 내 경로
        string prefabPath = "Items/ItemPrefab";
        GameObject newItem = PhotonNetwork.InstantiateRoomObject(prefabPath, position, Quaternion.identity);

        Debug.Log(itemPrefabs[itemIndex].name + " 아이템이 생성되었습니다!");
    }
}

