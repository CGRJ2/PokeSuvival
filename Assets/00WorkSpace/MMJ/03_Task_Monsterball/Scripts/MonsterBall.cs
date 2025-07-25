using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class MonsterBall : MonoBehaviourPunCallbacks
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

    [PunRPC]
    private void RPC_DropItem(Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("������ �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ���ͺ��� �ִ� ��ġ�� ������ ����
        GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);

        Debug.Log("�������� �����Ǿ����ϴ�!");
    }
}
