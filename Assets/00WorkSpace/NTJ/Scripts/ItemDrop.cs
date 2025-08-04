using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTJ
{
    public class ItemDrop : MonoBehaviourPun
    {
        [SerializeField] private List<ItemDropEntry> dropTable;

        public void OnDeath()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (dropTable == null || dropTable.Count == 0)
            {
                Debug.LogWarning("드롭 테이블이 비어 있음");
                return;
            }

            int selectedIndex = GetRandomIndex();
            if (selectedIndex < 0) return;

            int itemId = dropTable[selectedIndex].itemData.id;
            Vector3 spawnPos = transform.position;
            SpawnItem(spawnPos, itemId);
        }

        private int GetRandomIndex()
        {
            float total = dropTable.Sum(e => e.dropProbability);
            float rand = Random.value * total;
            float cumulative = 0f;

            for (int i = 0; i < dropTable.Count; i++)
            {
                cumulative += dropTable[i].dropProbability;
                if (rand <= cumulative)
                    return i;
            }

            return -1;
        }

        private void SpawnItem(Vector3 position, int itemId)
        {
            object[] data = new object[] { itemId };
            PhotonNetwork.Instantiate("Items/ItemPrefab", position, Quaternion.identity, 0, data);
        }
    }
}