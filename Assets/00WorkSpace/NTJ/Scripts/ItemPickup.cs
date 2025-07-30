using Photon.Pun;
using UnityEngine;

namespace NTJ
{
    public class ItemPickup : MonoBehaviourPun
    {
        private int itemId;
        private SpriteRenderer spriteRenderer;
        private bool isPickedUp = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 아이템 ID로 초기화하고, Sprite도 설정
        public void Initialize(int id)
        {
            itemId = id;
            isPickedUp = false; // 풀에서 나올 때 초기화
            var data = ItemDatabase.Instance.GetItemById(id);

            if (data == null)
            {
                Debug.LogWarning($"[ItemPickup] 유효하지 않은 itemId: {id}");
                return;
            }

            if (data.sprite == null)
            {
                Debug.LogWarning($"[ItemPickup] itemId={id}에 스프라이트가 없습니다.");
                return;
            }

            spriteRenderer.sprite = data.sprite;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return; // 중복 방지
            isPickedUp = true;

            if (!other.TryGetComponent<PhotonView>(out var targetView)) return;

            photonView.RPC(nameof(ApplyItemEffect), RpcTarget.AllBuffered, itemId, targetView.ViewID);
            ItemObjectPool.Instance.ReturnToPool(this);
        }

        [PunRPC]
        private void ApplyItemEffect(int id, int viewID)
        {
            var data = ItemDatabase.Instance.GetItemById(id);
            var receiver = PhotonView.Find(viewID)?.GetComponent<IStatReceiver>();

            if (data == null || receiver == null)
            {
                Debug.LogWarning("[ItemPickup] 아이템 효과 적용 실패");
                return;
            }

            receiver.ApplyStat(data);
        }
    }
}