using NTJ;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ItemObjectPool : MonoBehaviourPun
{
    public static ItemObjectPool Instance;

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int poolSize = 20;

    public Queue<ItemPickup> Pool = new();
	public ItemDatabase ItemDatabase { get; private set; }
    [Header("아이템 DB")]
	public ItemData[] items;
	private Dictionary<int, ItemData> _itemDict;

	private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // 싱글톤 안전 처리

        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(itemPrefab, transform); // TODO : 룸오브젝트로 생성
            obj.SetActive(false);
            Pool.Enqueue(obj.GetComponent<ItemPickup>());
        }
    }

    public ItemPickup SpawnItem(Vector3 position, int itemId)
    {
        ItemPickup item;

        if (Pool.Count > 0)
        {
            item = Pool.Dequeue();
        }
        else
        {
            var go = Instantiate(itemPrefab, transform);
            item = go.GetComponent<ItemPickup>();
        }

        item.transform.position = position;
        item.Initialize(itemId);
        item.gameObject.SetActive(true);

        // 공유 아이템으로 누구나 먹을 수 있도록 Scene Ownership 설정
        item.photonView.TransferOwnership(0); // 0은 Scene의 ViewID

        return item;
    }

    public void ReturnToPool(ItemPickup item)
    {
        if (!Pool.Contains(item))
        {
            item.gameObject.SetActive(false);
            Pool.Enqueue(item);
        }
    }

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
		if (!PhotonNetwork.IsMasterClient) return null;
        if (ItemDatabase == null) ItemDatabaseInit();
		return _itemDict != null && _itemDict.TryGetValue(id, out var data) ? data : null;
	}
}
