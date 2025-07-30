using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
    public class ItemDrop : MonoBehaviourPun
    {
        [SerializeField] private List<ItemData> possibleDrops;
        [SerializeField][Range(0f, 1f)] private float dropChance = 0.3f;

        public void OnDeath()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (possibleDrops == null || possibleDrops.Count == 0)
            {
                Debug.LogWarning("[ItemDrop] 드롭 가능한 아이템이 없음.");
                return;
            }

            if (Random.value < dropChance)
            {
                int index = Random.Range(0, possibleDrops.Count);
                int itemId = possibleDrops[index].id;

                var item = ItemObjectPool.Instance.SpawnItem(transform.position, itemId);

                if (item.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 randomForce = Random.insideUnitCircle.normalized * 3f;
                    rb.AddForce(randomForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}