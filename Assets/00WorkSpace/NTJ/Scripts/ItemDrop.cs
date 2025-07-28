using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace NTJ
{
    public class ItemDrop : MonoBehaviourPun
    {
        [SerializeField] private List<ItemData> possibleDrops;
        [SerializeField] private float dropChance = 0.3f;

        public void OnDeath()
        {
            if (PhotonNetwork.IsMasterClient && Random.value < dropChance)
            {
                int index = Random.Range(0, possibleDrops.Count);
                GameObject drop = PhotonNetwork.Instantiate("ItemPrefab", transform.position, Quaternion.identity);

                ItemPickup pickup = drop.GetComponent<ItemPickup>();
                pickup.Initialize(possibleDrops[index]);

                if (drop.TryGetComponent<Rigidbody2D>(out var rb))  // 2D
                {
                    Vector2 randomForce = Random.insideUnitCircle.normalized * 3f;
                    rb.AddForce(randomForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}