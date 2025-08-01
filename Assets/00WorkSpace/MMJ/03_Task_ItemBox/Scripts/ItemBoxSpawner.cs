using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemBoxSpawner : MonoBehaviourPunCallbacks
{
    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnTimer;

    [Header("타일맵 설정")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private float spawnHeight = 0f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();

    private void Awake()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("ItemBoxSpawner: 타일맵이 비어 있습니다!");
            enabled = false;
            return;
        }

        CollectValidTileSpawnPositions();
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear();
        BoundsInt bounds = targetTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (targetTilemap.HasTile(cellPos))
                {
                    Vector3 worldPos = targetTilemap.CellToWorld(cellPos);
                    worldPos.z = spawnHeight;
                    validSpawnPositions.Add(worldPos);
                }
            }
        }

        Debug.Log($"유효한 스폰 위치 {validSpawnPositions.Count}개 확보됨");
    }

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnItemBox();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnItemBox()
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("스폰할 위치가 없습니다.");
            return;
        }

        Vector3 spawnPosition = validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
        PhotonObjectPoolManager.Instance.Spawn("ItemBox", spawnPosition, Quaternion.identity);
    }
}