using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Runtime.Serialization;


public class ItemBox : MonoBehaviourPun, IPunObservable, IDamagable
{
    [Header("������ ����(��� Ȯ�� ������ �����Դϴ�.)")]
    [SerializeField] private GameObject[] itemPrefabs; // ���� ������ ������ �迭�� ����
    [SerializeField] private float[] dropProbabilities; // �� �������� ��� Ȯ��

    [Header("��Ʋ ������")]

    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;

    // IDamagable �������̽� ����
    public BattleDataTable BattleData
    {
        get => new BattleDataTable(
            -1,
            null,
            default,
            maxHp,
            currentHp);
    }

    // �������� �޴� �޼��� ����
    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        currentHp -= damage;

        // ü���� 0 ���ϸ� �ı�
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        throw new System.NotImplementedException();
    }

    // ��� ó�� �޼��� ����
    public void Die()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnHit();
        }
    }


    // �÷��̾� ���ݿ� ���� �ı��� �� ȣ��
    public void OnHit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ���ͺ��� ���� ��ġ ����
            Vector3 spawnPosition = transform.position;

            // ���� ������ ���� �� ���
            int selectedItemIndex = GetRandomItemIndex();

            // ���õ� ������ �ε����� RPC�� ����
            photonView.RPC(nameof(RPC_DropItem), RpcTarget.AllBuffered, spawnPosition, selectedItemIndex);

            // Ǯ�� ��ȯ
            ItemBoxPoolManager.Instance.ReturnToPool(gameObject);
        }
    }

    // Ȯ���� ���� ���� ������ �ε��� ����
    private int GetRandomItemIndex()
    {
        // �������̳� Ȯ�� �迭�� ���ų� ���̰� �ٸ��� -1 ��ȯ
        if (itemPrefabs == null || dropProbabilities == null ||
            itemPrefabs.Length == 0 || itemPrefabs.Length != dropProbabilities.Length)
        {
            Debug.LogWarning("������ ������ �Ǵ� Ȯ�� ������ �ùٸ��� �ʽ��ϴ�.");
            return -1;
        }

        // Ȯ�� �հ� ��� (���� 1�� �ƴ� �� �����Ƿ�)
        float totalProbability = 0f;
        foreach (float prob in dropProbabilities)
        {
            totalProbability += prob;
        }

        // 0~1 ������ ���� �� ����
        float randomValue = Random.value * totalProbability;

        // Ȯ���� ���� ������ ����
        float cumulativeProbability = 0f;

        for (int i = 0; i < dropProbabilities.Length; i++)
        {
            cumulativeProbability += dropProbabilities[i];

            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        // �⺻������ ù ��° ������ ��ȯ
        return 0;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // �� �����͸� ����
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(currentHp);
        }
        else
        {
            // ������ �����͸� ����
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            currentHp = (int)stream.ReceiveNext();
        }
    }


    private void OnTriggerEnter2D(Collider2D collider) // �׽�Ʈ�� �ڵ�
    {
        // �÷��̾ ������ �������� ����
        if (collider.CompareTag("Player"))
        {
            // �ڱ� �ڽ�(������)�� ��Ȱ��ȭ
            ItemBoxPoolManager.Instance.DeactivateObject(this.gameObject);
        }
    }

    [PunRPC]
    private void RPC_DropItem(Vector3 position, int itemIndex)
    {
        // ������ �ε����� ��ȿ���� ������ ����
        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
        {
            Debug.LogWarning("��ȿ���� ���� ������ �ε����Դϴ�: " + itemIndex);
            return;
        }

        // ���õ� ������ ������ ����
        string prefabPath = "Items/" + itemPrefabs[itemIndex].name; // Resources ���� �� ���
        GameObject newItem = PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity);

        Debug.Log(itemPrefabs[itemIndex].name + " �������� �����Ǿ����ϴ�!");
    }
}

