using NTJ;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UIElements;


public class ItemBox : MonoBehaviourPun, IPunObservable, IDamagable
{
    [Header("아이템 설정(드롭 확률 변수는 비율입니다.)")]

    [SerializeField] private List<ItemDropEntry> dropTable;

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
        Debug.Log("방장이 아이템박스가 부셔졌을 때 아이템 랜덤 드롭");

        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 spawnPosition = transform.position;
            ItemDropEntry selectedEntry = GetRandomDropEntry();

            if (selectedEntry != null)
            {
                int itemId = selectedEntry.itemData.id;
                photonView.RPC(nameof(RPC_DropItem), RpcTarget.All, spawnPosition, itemId);
            }

            // 스포너 알림
            ItemBoxSpawner.Instance?.ItemBoxDestroyed();

            PhotonNetwork.Destroy(gameObject);
        }
    }


    private ItemDropEntry GetRandomDropEntry()
    {
        if (dropTable == null || dropTable.Count == 0)
        {
            Debug.LogWarning("드롭 테이블이 비어 있습니다.");
            return null;
        }

        float total = dropTable.Sum(e => e.dropProbability);
        float rand = Random.value * total;
        float cumulative = 0f;

        foreach (var entry in dropTable)
        {
            cumulative += entry.dropProbability;
            if (rand <= cumulative)
                return entry;
        }

        // fallback
        Debug.LogWarning("드롭 확률 계산 이상: fallback 반환");
        return dropTable[dropTable.Count - 1]; // 마지막 항목을 fallback으로
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    [PunRPC]
    public void RPC_DropItem(Vector3 position, int itemId)
    {
        try
        {
            Debug.Log($"RPC_DropItem 호출: 위치={position}, 아이템ID={itemId}");

            ItemDropEntry entry = dropTable.FirstOrDefault(e => e.itemData.id == itemId);
            if (entry == null)
            {
                Debug.LogError($"해당 ID의 아이템이 드롭 테이블에 없습니다: ID={itemId}");
                return;
            }

            GameObject newItem = PhotonNetwork.InstantiateRoomObject("Item", position, Quaternion.identity);
            if (newItem != null && newItem.TryGetComponent<ItemPickup>(out var itemPickup))
            {
                itemPickup.Initialize(itemId);
                Debug.Log($"아이템 생성 성공: {itemId}");
            }
            else
            {
                // 왜 여기로 빠지는지는 모르겠음.. 주석처리하면 해결됨 Debug.LogError("아이템 생성 실패 또는 ItemPickup 컴포넌트 없음!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"RPC_DropItem 예외 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }
}