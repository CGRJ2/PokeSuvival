using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Runtime.Serialization;


public class ItemBox : MonoBehaviourPun, IPunObservable
{
    [Header("������ ����")]
    [SerializeField] private GameObject itemPrefab;



    // �÷��̾� ���ݿ� ���� �ı��� �� ȣ��
    public void OnHit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ���ͺ��� ���� ��ġ ����
            Vector3 spawnPosition = transform.position;

            // ������ ��� ���� - ��ġ ������ �Բ� ����
            photonView.RPC(nameof(RPC_DropItem), RpcTarget.AllBuffered, spawnPosition);

            // Ǯ�� ��ȯ
            ItemBoxPoolManager.Instance.ReturnToPool(gameObject);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // �� �����͸� ����
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // ������ �����͸� ����
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // �÷��̾ ������ �������� ����
        if (collider.CompareTag("Player"))
        {
            // �ڱ� �ڽ�(������)�� ��Ȱ��ȭ
            ItemBoxPoolManager.Instance.DeactivateObject(this.gameObject);
        }
    }

    [PunRPC]
    private void RPC_DropItem(Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("������ �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        GameObject newItem = PhotonNetwork.Instantiate("ItemPrefab", position, Quaternion.identity);

        Debug.Log("�������� �����Ǿ����ϴ�!");
    }
}
