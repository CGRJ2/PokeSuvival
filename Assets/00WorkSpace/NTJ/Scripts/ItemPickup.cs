using Photon.Pun;
using UnityEngine;

namespace NTJ
{
    public class ItemPickup : MonoBehaviourPun
    {
		[SerializeField] private int itemId;
		[SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool isPickedUp = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 아이템 ID로 초기화하고, Sprite도 설정
        [PunRPC]
        public void RPC_Initialize(int id)
        {
            Debug.Log("클라 아이템 초기화 시작");
            itemId = id;
            isPickedUp = false; // 풀에서 나올 때 초기화
            var data = ItemObjectPool.Instance.GetItemById(id);

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
            Debug.Log("클라 아이템 초기화 완료");
        }
		public void Initialize(int id)
		{
			Debug.Log("방장 아이템 초기화 시작");
			itemId = id;
			isPickedUp = false; // 풀에서 나올 때 초기화
			var data = ItemObjectPool.Instance.GetItemById(id);

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
			Debug.Log("방장 아이템 초기화 완료");

            photonView.RPC(nameof(RPC_Initialize), RpcTarget.OthersBuffered, id);
		}

		private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || isPickedUp) return;
            isPickedUp = true;

            if (!other.TryGetComponent<IStatReceiver>(out var player)) return;
			var data = ItemObjectPool.Instance.GetItemById(this.itemId);
            player.ApplyStat(data);
            photonView.RPC(nameof(RPC_ItemDespawn), RpcTarget.AllBuffered);
        }

        [PunRPC]
        public void RPC_ItemDespawn()
        {
            gameObject.SetActive(false);
            Debug.Log("아이템을 획득하여 비활성화 완료");
        }

		public void OnDisable()
		{
			ItemObjectPool.Instance.ReturnToPool(this);
            Debug.Log("비활성화 후 풀로 돌아가기 완료");
		}

		[PunRPC]
        private void ApplyItemEffect(int id, int viewID)
        {
            var data = ItemObjectPool.Instance.GetItemById(id);
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