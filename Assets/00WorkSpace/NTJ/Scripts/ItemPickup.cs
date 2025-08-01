using UnityEngine;
using Photon.Pun;
using NTJ;

public class ItemPickup : MonoBehaviourPun
{
    [SerializeField] private int itemId;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isPickedUp = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [PunRPC]
    public void RPC_Initialize(int id, Vector3 position)
    {
        itemId = id;
        isPickedUp = false;

        var data = ItemDatabaseManager.Instance.GetItemById(id);
        if (data == null) return;

        transform.position = position;
        spriteRenderer.sprite = data.sprite;
    }

    public void Initialize(int id)
    {
        itemId = id;
        isPickedUp = false;

        var data = ItemDatabaseManager.Instance.GetItemById(id);
        if (data == null) return;

        spriteRenderer.sprite = data.sprite;

        photonView.RPC(nameof(RPC_Initialize), RpcTarget.OthersBuffered, id, transform.position);
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
    }

    private void OnDisable()
    {
        PhotonObjectPoolManager.Instance.ReturnToPool("Item", gameObject);
    }
}