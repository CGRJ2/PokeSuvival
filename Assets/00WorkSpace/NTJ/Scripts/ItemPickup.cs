using Photon.Pun;
using UnityEngine;

namespace NTJ
{
    public class ItemPickup : MonoBehaviourPun
    {
        // SpriteRenderer를 만들고 ItemPickup에 할당
        // ItemPrefab에 PhotonView 컴포넌트를 붙이기

        private ItemData itemData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(ItemData data)
        {
            // 아이템 외형 설정
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