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
            Vector3 spawnPosition = transform.position;
            int selectedItemIndex = GetRandomItemIndex();

            photonView.RPC(nameof(RPC_DropItem), RpcTarget.All, spawnPosition, selectedItemIndex);

            // 중요: 파괴되기 전에 스포너에게 알림!
            if (ItemBoxSpawner.Instance != null) // << 새로 추가
            {
                ItemBoxSpawner.Instance.ItemBoxDestroyed(); // << 새로 추가
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }


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


        return 0;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    [PunRPC]
    public void RPC_DropItem(Vector3 position, int itemIndex)
    {
        try
        {
            Debug.LogError($"RPC_DropItem 호출: 위치={position}, 인덱스={itemIndex}");

            // 아이템 데이터 매니저 직접 참조 (싱글톤 의존성 제거)
            var dataManager = FindObjectOfType<ItemDataManager>();
            if (dataManager == null)
            {
                Debug.LogError("ItemDataManager를 찾을 수 없습니다!");
                return;
            }

            // 아이템 ID 계산 (기존 로직 유지)
            int realItemIndex = itemIndex + 1;

            // 아이템 데이터 가져오기
            ItemData itemData = dataManager.GetItemById(realItemIndex);
            if (itemData == null)
            {
                Debug.LogError($"아이템 데이터가 없습니다: ID={realItemIndex}");
                return;
            }

            // 아이템 생성 시도
            string prefabPath = "Item"; // Resources 폴더 내 경로
            GameObject newItem = PhotonNetwork.InstantiateRoomObject(prefabPath, position, Quaternion.identity);

            // 생성된 아이템 초기화
            if (newItem != null)
            {
                var itemPickup = newItem.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    itemPickup.Initialize(itemData.id);
                    Debug.LogError($"아이템 생성 성공: {itemData.id}");
                }
                else
                {
                    Debug.LogError("생성된 아이템에 ItemPickup 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogError("아이템 생성 실패!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"RPC_DropItem 예외 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }
}