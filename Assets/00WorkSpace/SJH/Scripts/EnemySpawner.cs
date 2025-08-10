using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private int _minLevel = 1;
    [SerializeField] private int _maxLevel = 2;

    [SerializeField] private float _spawnDelay = 5f;
    [SerializeField] private int _spawnCount = 30;

    private Coroutine _spawnRoutine;
    private WaitForSeconds _spawnTime;
    [SerializeField] private List<Enemy> _enemiesPool;

    [SerializeField]


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _enemiesPool = new(_spawnCount);
        _spawnTime = new WaitForSeconds(_spawnDelay);
    }

    public void SpawnInit()
    {
        if (PhotonNetwork.IsMasterClient) StartSpawning();
    }

    void StartSpawning()
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("마스터 변경 스포너 재작동");
        if (PhotonNetwork.IsMasterClient) StartSpawning();
        else StopSpawning();
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return _spawnTime;

            if (!PhotonNetwork.IsMasterClient) yield break;

            if (_enemiesPool.Count < _spawnCount) EnemySpawn();
        }
    }

    public void EnemySpawn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 생성 위치
        Vector3 spawnPos = ExpOrbSpawner.Instance.GetRandomTilePosition();


        int r = Random.Range(0, 100);
        int level = 0;
        BackendManager bm = BackendManager.Instance;
        PokemonData selectedPokemon = null;
        if (r <= 85)
        {
            level = Random.Range(_minLevel, _maxLevel);

            // [하] 몬스터 리스트 중 랜덤 선택
            int rand = Random.Range(0, bm.pokemonDatas_LowGroup.Length);
            selectedPokemon = Define.GetPokeData(bm.pokemonDatas_LowGroup[rand]);
        }
        else if (r <= 95)
        {
            level = Random.Range(_maxLevel + 1, _maxLevel * 2);       // 13~24
                                                                      
            // [중] 몬스터 리스트 중 랜덤 선택
            int rand = Random.Range(0, bm.pokemonDatas_MidGroup.Length);
            selectedPokemon = Define.GetPokeData(bm.pokemonDatas_MidGroup[rand]);
        }
        else
        {
            level = Random.Range(_minLevel * 2 + 1, _maxLevel * 3);   // 25~36
                                                                      
            // [상] 몬스터 리스트 중 랜덤 선택
            int rand = Random.Range(0, bm.pokemonDatas_HighGroup.Length);
            selectedPokemon = Define.GetPokeData(bm.pokemonDatas_HighGroup[rand]);
        }

        if (selectedPokemon == null) return;

        // 도감 선택
        // 딕셔너리에서 n번째 포켓몬 데이터 반환
        PhotonNetwork.InstantiateRoomObject("Enemy", spawnPos, Quaternion.identity, 0,
            new object[]
            {
                selectedPokemon.PokeNumber,	// 도감 번호
				level					// 레벨
			});
    }

    public void AddPool(Enemy enemy)
    {
        if (!_enemiesPool.Contains(enemy)) _enemiesPool.Add(enemy);
    }

    public void RemovePool(Enemy enemy)
    {
        if (_enemiesPool.Contains(enemy)) _enemiesPool.Remove(enemy);
    }
}
