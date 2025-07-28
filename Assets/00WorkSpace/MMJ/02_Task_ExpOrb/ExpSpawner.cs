using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExpOrbSpawner : MonoBehaviour
{
    [SerializeField] private ExpOrbPool orbPool;
    [SerializeField] private int maxOrbs = 50;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnAmountPerInterval = 5;

    [Header("Ÿ�ϸ� ����")]
    [SerializeField] private Tilemap targetTilemap; // ������ Ÿ�ϸ� ����

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // ��ȿ�� ���� ��ġ���� ������ ����Ʈ
    private List<ExpOrb> activeOrbs = new List<ExpOrb>();

    private void Awake()
    {
        // Ÿ�ϸ��� �ν����Ϳ��� �Ҵ�Ǿ����� Ȯ��
        if (targetTilemap == null)
        {
            Debug.LogError("ExpOrbSpawner: ������ Ÿ�ϸ�(Target Tilemap)�� �Ҵ���� �ʾҽ��ϴ�! �ν����Ϳ��� �������ּ���.");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        // ���� ���� �� ��ȿ�� ���� ��ġ���� �̸� ã�� ����
        CollectValidTileSpawnPositions();
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear(); // ���� ����Ʈ�� ���ϴ�

        // Ÿ�ϸ��� ���� �� ��踦 �����ɴϴ�
        BoundsInt bounds = targetTilemap.cellBounds;
        Debug.Log($"Ÿ�ϸ� ���: {bounds.min} ~ {bounds.max}");

        // Ÿ�ϸ� ��� ���� ��� ���� �ݺ��ϸ� �˻�
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // ���� �˻��� ���� ��ǥ (2D Ÿ�ϸ��� Z=0)
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // �ش� ���� ������ Ÿ���� �׷��� �ִ��� Ȯ��
                if (targetTilemap.HasTile(cellPosition))
                {
                    // �� ��ǥ�� ���� ��ǥ�� ��ȯ
                    Vector3 worldPosition = targetTilemap.CellToWorld(cellPosition);

                    // Ÿ���� �߾ӿ� ��ġ�ϵ��� ���� (Ÿ�ϸ� ������ ���� ���� �ʿ��� �� ����)
                    worldPosition += new Vector3(0.5f, 0.5f, 0);

                    validSpawnPositions.Add(worldPosition);
                }
            }
        }

        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("Ÿ�ϸʿ��� ������ �� �ִ� ��ȿ�� Ÿ���� ã�� ���߽��ϴ�!");
        }
        else
        {
            Debug.Log($"Ÿ�ϸʿ��� {validSpawnPositions.Count}���� ��ȿ�� ���� ��ġ�� ã�ҽ��ϴ�.");
        }
    }

    private void Start()
    {
        // ��ȿ�� ���� ��ġ�� ������ �������� ����
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogError("��ȿ�� ���� ��ġ�� ���� ����ġ ������ ������ �� �����ϴ�.");
            return;
        }

        SpawnOrbsImmediate(); // ó���� �ѹ濡 �Ѹ��� �ϴ� ����׿�
        InvokeRepeating(nameof(SpawnOrbs), 1f, spawnInterval);
    }

    private void SpawnOrbsImmediate()
    {
        int spawnCount = maxOrbs;

        for (int i = 0; i < spawnCount; i++)
        {
            ExpOrb orb = orbPool.GetOrb();
            orb.transform.position = GetRandomTilePosition();
            orb.Init(Random.Range(1f, 3f));
            orb.gameObject.SetActive(true);
            activeOrbs.Add(orb);

            // �ߺ� ���� ����
            orb.OnDespawned -= HandleOrbDespawned;
            orb.OnDespawned += HandleOrbDespawned;
        }
    }

    private void SpawnOrbs()
    {
        int currentCount = activeOrbs.Count;

        if (currentCount >= maxOrbs)
            return;

        int spawnCount = Mathf.Min(spawnAmountPerInterval, maxOrbs - currentCount);

        for (int i = 0; i < spawnCount; i++)
        {
            ExpOrb orb = orbPool.GetOrb();
            orb.transform.position = GetRandomTilePosition();
            orb.Init(Random.Range(1f, 3f));
            orb.gameObject.SetActive(true);
            activeOrbs.Add(orb);
            orb.OnDespawned += HandleOrbDespawned;
        }
    }

    // Ÿ�ϸ��� ��ȿ�� ��ġ �� ������ ��ġ ��ȯ
    private Vector3 GetRandomTilePosition()
    {
        if (validSpawnPositions.Count == 0)
            return Vector3.zero;

        int randomIndex = Random.Range(0, validSpawnPositions.Count);
        return validSpawnPositions[randomIndex];
    }

    private void HandleOrbDespawned(ExpOrb orb)
    {
        if (activeOrbs.Contains(orb))
        {
            activeOrbs.Remove(orb);
            orb.OnDespawned -= HandleOrbDespawned;
            orbPool.ReturnOrb(orb); // �߰��� ��ȯ����
        }
    }
}