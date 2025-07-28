using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class ItemBoxSpawner : MonoBehaviourPunCallbacks
{
    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 5f;

    [Header("타일맵 설정")]
    [SerializeField] private Tilemap targetTilemap; // 스폰할 타일맵 참조
    [SerializeField] private float spawnHeight = 0f; // 2D 게임에서 아이템이 생성될 Z축 높이
    private float spawnTimer;
    private List<Vector3> validSpawnPositions = new List<Vector3>(); // 유효한 스폰 위치들을 저장할 리스트

    void Awake()
    {
        Debug.Log("enabled 상태는? " + enabled);  // 여기에 true인지 false인지 출력

        // 타일맵이 인스펙터에서 할당되었는지 확인합니다.
        if (targetTilemap == null)
        {
            Debug.LogError("MonsterBallSpawner: 스폰할 타일맵(Target Tilemap)이 할당되지 않았습니다! 인스펙터에서 설정해주세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 게임 시작 시 (또는 씬 로딩 시) 유효한 스폰 위치들을 미리 찾아 저장합니다.
        CollectValidTileSpawnPositions();
    }

    private void CollectValidTileSpawnPositions()
    {
        validSpawnPositions.Clear(); // 기존 리스트를 비웁니다.

        // 타일맵의 현재 셀 경계를 가져옵니다. (타일이 그려진 최소/최대 X, Y 범위)
        BoundsInt bounds = targetTilemap.cellBounds;

        // 타일맵 경계 내의 모든 셀을 반복하며 검사합니다.
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // 현재 검사할 셀의 좌표 (2D 타일맵은 Z=0)
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // 해당 셀에 실제로 타일이 그려져 있는지 확인합니다
                if (targetTilemap.HasTile(cellPosition))
                {
                    // 셀 좌표를 월드 좌표로 변환합니다
                    Vector3 worldPosition = targetTilemap.CellToWorld(cellPosition);

                    // 2D 게임이므로 Z축(깊이) 값은 보통 0으로 고정하거나, 설정된 spawnHeight 값을 사용합니다.
                    worldPosition.z = spawnHeight;

                    validSpawnPositions.Add(worldPosition);
                }

            }
        }

        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("타일맵에서 스폰할 수 있는 유효한 타일을 찾지 못했습니다! 타일맵이 비어있거나 스폰 조건에 맞는 타일이 없습니다.");
        }
        else
        {
            Debug.Log($"타일맵에서 {validSpawnPositions.Count}개의 유효한 스폰 위치를 찾았습니다.");
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
            // Debug.Log("나는 마스터 클라이언트다! 스폰을 시도한다.");
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                // Debug.Log("타이머 완료. SpawnMonsterBall 호출!");
                SpawnMonsterBall();
                spawnTimer = spawnInterval;
            }
        }
    }

    private void SpawnMonsterBall()
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("스폰 가능한 타일 위치가 없습니다. 몬스터볼을 스폰할 수 없습니다.");
            return;
        }

        // 유효한 스폰 위치 리스트에서 랜덤하게 하나를 선택합니다.
        int randomIndex = Random.Range(0, validSpawnPositions.Count);
        Vector3 spawnPosition = validSpawnPositions[randomIndex];

        // 풀 매니저에 스폰 요청
        ItemBoxPoolManager.Instance.SpawnMonsterBall(spawnPosition);
        Debug.Log($"몬스터볼이 타일맵의 유효한 위치에 스폰되었습니다: {spawnPosition}");
    }
}
