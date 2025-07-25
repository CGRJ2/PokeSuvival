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

            // ������ ��� ����
            photonView.RPC("RPC_DropItem", RpcTarget.All);

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
