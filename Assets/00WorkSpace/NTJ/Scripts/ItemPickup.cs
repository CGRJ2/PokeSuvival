using Photon.Pun;
using UnityEngine;

namespace NTJ
{
    public class ItemPickup : MonoBehaviourPun
    {
        // SpriteRenderer�� ����� ItemPickup�� �Ҵ�
        // ItemPrefab�� PhotonView ������Ʈ�� ���̱�

        private ItemData itemData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(ItemData data)
        {
            // ������ ���� ����
            itemData = data;

            if (spriteRenderer != null && itemData.sprite != null)
            {
                spriteRenderer.sprite = itemData.sprite;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IStatReceiver>(out var receiver))
            {
                receiver.ApplyStat(itemData);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}