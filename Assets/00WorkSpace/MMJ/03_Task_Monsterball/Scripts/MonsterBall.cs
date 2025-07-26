using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;


public class MonsterBall : MonoBehaviourPun, IPunObservable
{
    [Header("������ ����")]
    [SerializeField] private GameObject itemPrefab; // ������ ������ ������

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
            MonsterBallPoolManager.Instance.ReturnToPool(gameObject);
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

    [PunRPC]
    private void RPC_DropItem(Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("������ �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ���ͺ��� �ִ� ��ġ�� ������ ����
        // [[���� �ذ��� ���� �ּ�ó��333]]GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);
        GameObject newItem = PhotonNetwork.Instantiate("ItemPrefab", position, Quaternion.identity);

        Debug.Log("�������� �����Ǿ����ϴ�!");
    }
}
