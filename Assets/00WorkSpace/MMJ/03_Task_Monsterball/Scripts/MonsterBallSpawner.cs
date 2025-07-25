using UnityEngine;
using Photon.Pun;

public class MonsterBallSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY = 50f;
    [SerializeField] private float spawnHeight = 1f; // �ٴ� �� ����

    private float spawnTimer;

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        // ������ Ŭ���̾�Ʈ�� ���� ���� ����
        if (PhotonNetwork.IsMasterClient)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                SpawnMonsterBall();
                spawnTimer = spawnInterval;
            }
        }
    }

    private void SpawnMonsterBall()
    {
        // ���� ��ġ ���� (-50,50)
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(randomX, randomY, spawnHeight); // 2D ������ Z=0

        // Ǯ �Ŵ����� ���� ��û
        MonsterBallPoolManager.Instance.SpawnMonsterBall(spawnPosition);
    }
}
