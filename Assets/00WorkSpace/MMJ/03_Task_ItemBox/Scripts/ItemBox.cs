using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Runtime.Serialization;
using NTJ;
using System.Collections;


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
        //if (!PhotonNetwork.IsMasterClient) // 7.31 만준 수정사항 2 체력 계산을 마스터만
        //    return true;
        Debug.Log($"[TakeDamage] Called on {gameObject.name}, IsMaster: {PhotonNetwork.IsMasterClient}, IsMine: {photonView.IsMine}");
        // 스킬과 공격 데이터를 기반으로 데미지 계산
        int damage = 4;
        currentHp -= damage; // TODO : 나중에는 랜덤으로
		PlayerManager.Instance?.ShowDamageText(transform, damage, Color.white);

        photonView.RPC(nameof(RPC_SyncHp), RpcTarget.OthersBuffered, currentHp);

        // 체력이 0 이하면 파괴
        if (currentHp <= 0)
        {
                Debug.Log("아이템박스 부셔짐 방장 호출");
                photonView.RPC(nameof(RPC_OnHit), RpcTarget.AllBuffered);
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
        // 모든 클라이언트에서 박스 꺼짐 처리
        Debug.Log("아이템 박스 파괴됨 (동기화)");

        // 애니메이션이나 파티클 이펙트 있으면 여기 처리

        gameObject.SetActive(false);

        // 아이템 드롭은 마스터 클라이언트만 수행
        if (PhotonNetwork.IsMasterClient)
        {
            int selectedItemIndex = GetRandomItemIndex();
            Vector3 spawnPosition = transform.position;
            photonView.RPC(nameof(RPC_DropItem), RpcTarget.AllBuffered, spawnPosition, selectedItemIndex);
        }

        // 반환은 마스터 클라이언트만 (풀 시스템)
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonObjectPoolManager.Instance.ReturnToPool("ItemBox", gameObject);
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
        //if (stream.IsWriting)
        //{
        //    // 내 데이터를 보냄
        //    stream.SendNext(transform.position);
        //    stream.SendNext(transform.rotation);
        //    stream.SendNext(currentHp);
        //}
        //else
        //{
        //    // 상대방의 데이터를 받음
        //    transform.position = (Vector3)stream.ReceiveNext();
        //    transform.rotation = (Quaternion)stream.ReceiveNext();
        //    currentHp = (int)stream.ReceiveNext();
        //}
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
    public void RPC_DropItem(Vector3 position, int itemIndex)
    {
      //기존 코드
        // 아이템 인덱스가 유효하지 않으면 리턴
       // if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
       // {
       //     Debug.LogWarning("유효하지 않은 아이템 인덱스입니다: " + itemIndex);
       //     return;
       // }
       //
       // // itemIndex에 따라 ItemDatabase에서 받아옴
       // int realItemIndex = itemIndex + 1;
       // var io = ItemObjectPool.Instance;
       // if (io == null) return;
       // ItemData itemData = io.GetItemById(realItemIndex);
       // if (itemData == null)
       // {
       //     Debug.Log("아이템 데이터가 비어있습니다. 생성 실패");
       //     return;
       // }
       //
       // // 아이템 프리팹에 스프라이트나 데이터를 어디서 넣어줘야할지
       //
       // // 선택된 아이템 프리팹 생성
       // Debug.Log($"{realItemIndex}번째 아이템[{itemPrefabs[itemIndex].name}] 생성 시도");
       // GameObject itemObj = PhotonNetwork.InstantiateRoomObject.PhotonObjectPoolManager.Instance.Spawn("Item", position, Quaternion.identity);
       // ItemPickup itemPickup = itemObj.GetComponent<ItemPickup>();
       // itemPickup.Initialize(itemData.id);
       //
       // Debug.Log(itemPrefabs[itemIndex].name + " 아이템이 생성되었습니다!");



        if (!PhotonNetwork.IsMasterClient) return;

        GameObject itemObj = PhotonObjectPoolManager.Instance.Spawn("Item", position, Quaternion.identity);

        if (itemObj == null) return;

        int realItemIndex = itemIndex + 1;
        ItemData itemData = ItemObjectPool.Instance.GetItemById(realItemIndex);

        var photonView = itemObj.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC(nameof(ItemPickup.RPC_Initialize), RpcTarget.AllBuffered, itemData.id, position);
        }
    }
}

