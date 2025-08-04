using NTJ;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDataManager : MonoBehaviourPun
{
    public static ItemDataManager Instance { get; private set; }

    [SerializeField] private string itemPrefabPath = "Item"; // Resources 폴더 경로
    [SerializeField] private int maxActiveItems = 30; // 최대 활성화 아이템 수
    private int currentActiveItems = 0; // 현재 활성화된 아이템 수

    public ItemDatabase ItemDatabase { get; private set; }
    [Header("아이템 DB")]
    public ItemData[] items;
    private Dictionary<int, ItemData> _itemDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // 싱글톤 안전 처리
    }

    public ItemPickup SpawnItem(Vector3 position, int itemId)
    {
        // 최대 개수 제한 확인
        if (PhotonNetwork.IsMasterClient && currentActiveItems >= maxActiveItems)
        {
            Debug.Log($"현재 아이템 최대 개수({maxActiveItems})에 도달하여 생성하지 않습니다.");
            return null;
        }

        // 풀링 대신 직접 생성
        GameObject itemObj = PhotonNetwork.InstantiateRoomObject(itemPrefabPath, position, Quaternion.identity);
        ItemPickup item = itemObj.GetComponent<ItemPickup>();

        // 초기화 및 활성화
        item.Initialize(itemId);

        // 공유 아이템으로 누구나 먹을 수 있도록 Scene Ownership 설정
        item.photonView.TransferOwnership(0); // 0은 Scene의 ViewID

        // 아이템 개수 증가 (마스터 클라이언트만)
        if (PhotonNetwork.IsMasterClient)
        {
            currentActiveItems++;
            Debug.Log($"현재 활성화된 아이템 개수: {currentActiveItems}/{maxActiveItems}");
        }

        return item;
    }

    // 아이템 파괴 시 호출될 메서드 추가
    public void ItemDestroyed()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentActiveItems = Mathf.Max(0, currentActiveItems - 1);
            Debug.Log($"아이템 파괴됨. 현재 활성화된 아이템 개수: {currentActiveItems}/{maxActiveItems}");
        }
    }

    // 나머지 메서드들...
    public void ItemDatabaseInit()
    {
        ItemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
        _itemDict = ItemDatabase.items.ToDictionary(i => i.id);
        foreach (var kvp in _itemDict)
        {
            Debug.Log($"Key : {kvp.Key} / Value : {kvp.Value.itemName}");
        }
        Debug.Log($"아이템 데이터 {_itemDict.Count}개 초기화 완료");
    }

    public ItemData GetItemById(int id)
    {
        if (ItemDatabase == null) ItemDatabaseInit();
        return _itemDict != null && _itemDict.TryGetValue(id, out var data) ? data : null;
    }
}