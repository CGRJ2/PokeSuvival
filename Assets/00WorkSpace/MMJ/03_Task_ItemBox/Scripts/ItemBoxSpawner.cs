using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class ItemBoxSpawner : MonoBehaviourPunCallbacks
{
    private static ItemBoxSpawner _instance;




    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private string itemBoxPrefabPath = "ItemBox";
    [SerializeField] private int maxActiveItemBoxes = 30; // 최대 동시 활성화 아이템 박스 수
    private int currentActiveItemBoxes = 0; // 현재 활성화된 아이템 박스 수

    [Header("타일맵 설정")]
    [SerializeField] private Tilemap targetTilemap; // 스폰할 타일맵 참조

    [SerializeField] private float spawnHeight = 0f; // 2D 게임에서 아이템이 생성될 Z축 높이
    [SerializeField] private float spawnTimer;
    private List<Vector3> validSpawnPositions = new List<Vector3>(); // 유효한 스폰 위치들을 저장할 리스트




    void Awake()
    {
        if (_instance == null) // << 수정
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else if (_instance != this) // << 수정
        {
            Destroy(gameObject);
        }
        Debug.Log("enabled 상태는? " + enabled);  // 여기에 true인지 false인지 출력

        // 타일맵이 인스펙터에서 할당되었는지 확인합니다.
        if (targetTilemap == null)
        {
            Debug.LogError("ItemBoxSpawner: 스폰할 타일맵(Target Tilemap)이 할당되지 않았습니다! 인스펙터에서 설정해주세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 게임 시작 시 (또는 씬 로딩 시) 유효한 스폰 위치들을 미리 찾아 저장합니다.
        CollectValidTileSpawnPositions();
    }

    public static ItemBoxSpawner Instance // << 추가
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ItemBoxSpawner>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ItemBoxSpawner");
                    _instance = go.AddComponent<ItemBoxSpawner>();
                }
            }
            return _instance;
        }
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
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                SpawnItemBox();
                spawnTimer = spawnInterval;
            }
        }
    }

    public void ItemBoxDestroyed()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentActiveItemBoxes = Mathf.Max(0, currentActiveItemBoxes - 1); // 0 미만으로 내려가지 않도록 방지
            Debug.Log($"아이템 박스 파괴됨. 현재 활성화된 아이템 박스 개수: {currentActiveItemBoxes}");
        }
    }

    private void SpawnItemBox()
    {
        if (currentActiveItemBoxes >= maxActiveItemBoxes)
        {
            Debug.Log($"현재 아이템 박스 최대 개수({maxActiveItemBoxes})에 도달하여 생성하지 않습니다.");
            return; // 최대 개수에 도달했으면 더 이상 진행하지 않습니다.
        }

        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("스폰 가능한 타일 위치가 없습니다. 아이템박스를 스폰할 수 없습니다.");
            return;
        }

        // 유효한 스폰 위치 리스트에서 랜덤하게 하나를 선택합니다.
        int randomIndex = Random.Range(0, validSpawnPositions.Count);
        Vector3 spawnPosition = validSpawnPositions[randomIndex];

        // 오브젝트 풀링 대신 PhotonNetwork.Instantiate 사용
        GameObject itemBox = PhotonNetwork.InstantiateRoomObject(itemBoxPrefabPath, spawnPosition, Quaternion.identity);
        Debug.Log("아이템 박스 생성됨: " + spawnPosition);

        if (PhotonNetwork.IsMasterClient)
        {
            currentActiveItemBoxes++;
            Debug.Log($"현재 활성화된 아이템 박스 개수: {currentActiveItemBoxes}");
        }
    }
}