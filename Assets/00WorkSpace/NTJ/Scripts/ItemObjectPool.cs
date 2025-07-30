using NTJ;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ItemObjectPool : MonoBehaviourPun
{
    public static ItemObjectPool Instance;

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int poolSize = 20;

    private readonly Queue<ItemPickup> pool = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // �̱��� ���� ó��

        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(itemPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<ItemPickup>());
        }
    }

    public ItemPickup SpawnItem(Vector3 position, int itemId)
    {
        ItemPickup item;

        if (pool.Count > 0)
        {
            item = pool.Dequeue();
        }
        else
        {
            var go = Instantiate(itemPrefab, transform);
            item = go.GetComponent<ItemPickup>();
        }

        item.transform.position = position;
        item.Initialize(itemId);
        item.gameObject.SetActive(true);

        // ���� ���������� ������ ���� �� �ֵ��� Scene Ownership ����
        item.photonView.TransferOwnership(0); // 0�� Scene�� ViewID

        return item;
    }

    public void ReturnToPool(ItemPickup item)
    {
        if (!pool.Contains(item))
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
    }
}
