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

        // ������ ID�� �ʱ�ȭ�ϰ�, Sprite�� ����
        public void Initialize(int id)
        {
            itemId = id;
            isPickedUp = false; // Ǯ���� ���� �� �ʱ�ȭ
            var data = ItemDatabase.Instance.GetItemById(id);

            if (data == null)
            {
                Debug.LogWarning($"[ItemPickup] ��ȿ���� ���� itemId: {id}");
                return;
            }

            if (data.sprite == null)
            {
                Debug.LogWarning($"[ItemPickup] itemId={id}�� ��������Ʈ�� �����ϴ�.");
                return;
            }

            spriteRenderer.sprite = data.sprite;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return; // �ߺ� ����
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
                Debug.LogWarning("[ItemPickup] ������ ȿ�� ���� ����");
                return;
            }

            receiver.ApplyStat(data);
        }
    }
}