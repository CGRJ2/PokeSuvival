using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.Tilemaps;
using System.Collections;

public class ExpOrbSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private int maxOrbs = 50;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnAmountPerInterval = 5;
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private int _orbMinExp;
    [SerializeField] private int _orbMaxExp;

    private List<Vector3> validSpawnPositions = new();
    private List<ExpOrb> activeOrbs = new();

    private void Awake()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("[ExpOrbSpawner] 타일맵이 없습니다!");
            enabled = false;
            return;
        }
        CollectValidTileSpawnPositions();
    }

    //(기존코드)
    //public override void OnJoinedRoom()
    //{
    //    if (!PhotonNetwork.IsMasterClient) return;
    //    SpawnOrbsImmediate();
    //    InvokeRepeating(nameof(SpawnOrbs), 1f, spawnInterval);
    //}

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(WaitAndSpawn());
    }
    
    IEnumerator WaitAndSpawn()
    {
        yield return new WaitUntil(() => PhotonObjectPoolManager.Instance != null &&
                                         PhotonObjectPoolManager.Instance.HasKey("ExpOrb"));

        SpawnOrbsImmediate(); // 풀 초기화가 끝났을 때만 실행
    }

    private void SpawnOrbsImmediate()
    {
        for (int i = 0; i < maxOrbs; i++) SpawnOneOrb();
    }

    private void SpawnOrbs()
    {
        int spawnCount = Mathf.Min(spawnAmountPerInterval, maxOrbs - activeOrbs.Count);
        for (int i = 0; i < spawnCount; i++) SpawnOneOrb();
    }

    private void SpawnOneOrb()
    {
        Vector3 spawnPos = GetRandomTilePosition();
        GameObject orbObj = PhotonObjectPoolManager.Instance.Spawn("ExpOrb", spawnPos, Quaternion.identity);
        if (orbObj == null) return;

        ExpOrb orb = orbObj.GetComponent<ExpOrb>();
        orb.Init(Random.Range(_orbMinExp, _orbMaxExp));
        activeOrbs.Add(orb);

        orb.OnDespawned -= HandleOrbDespawned;
        orb.OnDespawned += HandleOrbDespawned;
    }

    private void HandleOrbDespawned(ExpOrb orb)
    {
        activeOrbs.Remove(orb);
        PhotonObjectPoolManager.Instance.ReturnToPool("ExpOrb", orb.gameObject);
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear();
        BoundsInt bounds = targetTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                if (targetTilemap.HasTile(cell))
                    validSpawnPositions.Add(targetTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f));
            }
        }
    }

    private Vector3 GetRandomTilePosition()
    {
        if (validSpawnPositions.Count == 0) return Vector3.zero;
        return validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
    }
}