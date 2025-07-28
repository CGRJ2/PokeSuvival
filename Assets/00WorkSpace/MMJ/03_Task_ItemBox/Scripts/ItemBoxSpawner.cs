using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class ItemBoxSpawner : MonoBehaviourPunCallbacks
{
    [Header("���� ����")]
    [SerializeField] private float spawnInterval = 5f;

    [Header("Ÿ�ϸ� ����")]
    [SerializeField] private Tilemap targetTilemap; // ������ Ÿ�ϸ� ����
    [SerializeField] private float spawnHeight = 0f; // 2D ���ӿ��� �������� ������ Z�� ����
    private float spawnTimer;
    private List<Vector3> validSpawnPositions = new List<Vector3>(); // ��ȿ�� ���� ��ġ���� ������ ����Ʈ

    void Awake()
    {
        Debug.Log("enabled ���´�? " + enabled);  // ���⿡ true���� false���� ���

        // Ÿ�ϸ��� �ν����Ϳ��� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (targetTilemap == null)
        {
            Debug.LogError("MonsterBallSpawner: ������ Ÿ�ϸ�(Target Tilemap)�� �Ҵ���� �ʾҽ��ϴ�! �ν����Ϳ��� �������ּ���.");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        // ���� ���� �� (�Ǵ� �� �ε� ��) ��ȿ�� ���� ��ġ���� �̸� ã�� �����մϴ�.
        CollectValidTileSpawnPositions();
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear(); // ���� ����Ʈ�� ���ϴ�.

        // Ÿ�ϸ��� ���� �� ��踦 �����ɴϴ�. (Ÿ���� �׷��� �ּ�/�ִ� X, Y ����)
        BoundsInt bounds = targetTilemap.cellBounds;

        // Ÿ�ϸ� ��� ���� ��� ���� �ݺ��ϸ� �˻��մϴ�.
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // ���� �˻��� ���� ��ǥ (2D Ÿ�ϸ��� Z=0)
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // �ش� ���� ������ Ÿ���� �׷��� �ִ��� Ȯ���մϴ�
                if (targetTilemap.HasTile(cellPosition))
                {
                    // �� ��ǥ�� ���� ��ǥ�� ��ȯ�մϴ�
                    Vector3 worldPosition = targetTilemap.CellToWorld(cellPosition);

                    // 2D �����̹Ƿ� Z��(����) ���� ���� 0���� �����ϰų�, ������ spawnHeight ���� ����մϴ�.
                    worldPosition.z = spawnHeight;

                    validSpawnPositions.Add(worldPosition);
                }

            }
        }

        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("Ÿ�ϸʿ��� ������ �� �ִ� ��ȿ�� Ÿ���� ã�� ���߽��ϴ�! Ÿ�ϸ��� ����ְų� ���� ���ǿ� �´� Ÿ���� �����ϴ�.");
        }
        else
        {
            Debug.Log($"Ÿ�ϸʿ��� {validSpawnPositions.Count}���� ��ȿ�� ���� ��ġ�� ã�ҽ��ϴ�.");
        }
    }

    private void Start()
    {
        spawnTimer = spawnInterval;
    }


    

    private void Update()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            // Debug.Log("���� ������ Ŭ���̾�Ʈ��! ������ �õ��Ѵ�.");
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                // Debug.Log("Ÿ�̸� �Ϸ�. SpawnMonsterBall ȣ��!");
                SpawnMonsterBall();
                spawnTimer = spawnInterval;
            }
        }
    }

    private void SpawnMonsterBall()
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("���� ������ Ÿ�� ��ġ�� �����ϴ�. ���ͺ��� ������ �� �����ϴ�.");
            return;
        }

        // ��ȿ�� ���� ��ġ ����Ʈ���� �����ϰ� �ϳ��� �����մϴ�.
        int randomIndex = Random.Range(0, validSpawnPositions.Count);
        Vector3 spawnPosition = validSpawnPositions[randomIndex];

        // Ǯ �Ŵ����� ���� ��û
        ItemBoxPoolManager.Instance.SpawnMonsterBall(spawnPosition);
        Debug.Log($"���ͺ��� Ÿ�ϸ��� ��ȿ�� ��ġ�� �����Ǿ����ϴ�: {spawnPosition}");
    }
}
