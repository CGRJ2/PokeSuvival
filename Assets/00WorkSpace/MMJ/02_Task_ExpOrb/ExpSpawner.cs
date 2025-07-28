using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExpOrbSpawner : MonoBehaviour
{
    [SerializeField] private ExpOrbPool orbPool;
    [SerializeField] private int maxOrbs = 50;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnAmountPerInterval = 5;

    [Header("타일맵 설정")]
    [SerializeField] private Tilemap targetTilemap; // 스폰할 타일맵 참조

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // 유효한 스폰 위치들을 저장할 리스트
    private List<ExpOrb> activeOrbs = new List<ExpOrb>();

    private void Awake()
    {
        // 타일맵이 인스펙터에서 할당되었는지 확인
        if (targetTilemap == null)
        {
            Debug.LogError("ExpOrbSpawner: 스폰할 타일맵(Target Tilemap)이 할당되지 않았습니다! 인스펙터에서 설정해주세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 게임 시작 시 유효한 스폰 위치들을 미리 찾아 저장
        CollectValidTileSpawnPositions();
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear(); // 기존 리스트를 비웁니다

        // 타일맵의 현재 셀 경계를 가져옵니다
        BoundsInt bounds = targetTilemap.cellBounds;
        Debug.Log($"타일맵 경계: {bounds.min} ~ {bounds.max}");

        // 타일맵 경계 내의 모든 셀을 반복하며 검사
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // 현재 검사할 셀의 좌표 (2D 타일맵은 Z=0)
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // 해당 셀에 실제로 타일이 그려져 있는지 확인
                if (targetTilemap.HasTile(cellPosition))
                {
                    // 셀 좌표를 월드 좌표로 변환
                    Vector3 worldPosition = targetTilemap.CellToWorld(cellPosition);

                    // 타일의 중앙에 위치하도록 조정 (타일맵 설정에 따라 조정 필요할 수 있음)
                    worldPosition += new Vector3(0.5f, 0.5f, 0);

                    validSpawnPositions.Add(worldPosition);
                }
            }
        }

        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("타일맵에서 스폰할 수 있는 유효한 타일을 찾지 못했습니다!");
        }
        else
        {
            Debug.Log($"타일맵에서 {validSpawnPositions.Count}개의 유효한 스폰 위치를 찾았습니다.");
        }
    }

    private void Start()
    {
        // 유효한 스폰 위치가 없으면 스폰하지 않음
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogError("유효한 스폰 위치가 없어 경험치 구슬을 스폰할 수 없습니다.");
            return;
        }

        SpawnOrbsImmediate(); // 처음에 한방에 뿌리게 하는 디버그용
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

            // 중복 구독 방지
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

    // 타일맵의 유효한 위치 중 랜덤한 위치 반환
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
            orbPool.ReturnOrb(orb); // 추가로 반환까지
        }
    }
}