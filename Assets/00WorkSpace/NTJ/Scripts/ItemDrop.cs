using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
    public class ItemDrop : MonoBehaviourPun
    {
        [SerializeField] private List<ItemData> possibleDrops;
        [SerializeField][Range(0f, 1f)] private float dropChance = 0.3f;

        private void Awake()
        {
            if (possibleDrops == null || possibleDrops.Count == 0)
            {
                possibleDrops = new List<ItemData>(ItemDatabase.Instance.items);
            }
        }

        public void OnDeath()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (possibleDrops == null || possibleDrops.Count == 0)
            {
                Debug.LogWarning("[ItemDrop] ��� ������ �������� ����.");
                return;
            }

            if (Random.value < dropChance)
            {
                int index = Random.Range(0, possibleDrops.Count);
                int itemId = possibleDrops[index].id;

                SpawnItemWithForce(transform.position, itemId);
            }
        }
        public void SpawnItemWithForce(Vector3 position, int itemId)
        {
            var item = ItemObjectPool.Instance.SpawnItem(position, itemId);

            if (item != null && item.TryGetComponent<Rigidbody2D>(out var rb))
            {
                // ���� ������ ���� ���� ����
                Vector2 randomDir = Random.insideUnitCircle.normalized;

                // �� ũ�� ����
                float forceMagnitude = 3f;

                // �� ����
                rb.AddForce(randomDir * forceMagnitude, ForceMode2D.Impulse);
            }
        }
    }
}