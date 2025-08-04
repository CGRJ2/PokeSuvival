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
            isPickedUp = false;
            var data = ItemDataManager.Instance.GetItemById(id); // ItemObjectPool → ItemDataManager로 변경

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
            isPickedUp = false;
            var data = ItemDataManager.Instance.GetItemById(id); // ItemObjectPool → ItemDataManager로 변경

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
            var data = ItemDataManager.Instance.GetItemById(this.itemId); // ItemObjectPool → ItemDataManager로 변경
            player.ApplyStat(data);
            photonView.RPC(nameof(RPC_ItemDespawn), RpcTarget.AllBuffered);
        }

        [PunRPC]
        public void RPC_ItemDespawn()
        {
            // 아이템 카운터 감소 (파괴 전에 호출)
            if (PhotonNetwork.IsMasterClient)
            {
                ItemDataManager.Instance.ItemDestroyed();
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // 마스터 클라이언트가 아닌 경우, 로컬에서만 비활성화
                gameObject.SetActive(false);
            }
            Debug.Log("아이템을 획득하여 파괴 또는 비활성화 완료");
        }

        // 풀링 관련 OnDisable 메서드 제거
        // public void OnDisable() 메서드 삭제

        [PunRPC]
        private void ApplyItemEffect(int id, int viewID)
        {
            var data = ItemDataManager.Instance.GetItemById(id); // ItemObjectPool → ItemDataManager로 변경
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