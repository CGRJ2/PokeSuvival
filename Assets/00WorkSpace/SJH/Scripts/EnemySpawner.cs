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

		// 레벨 선택
		int level = Random.Range(_minLevel, _maxLevel);

		// 도감 선택
		// 딕셔너리에서 n번째 포켓몬 데이터 반환
		PokemonData pokeData = Define.NumberToPokeData.ElementAt(Random.Range(0, Define.NumberToPokeData.Count)).Value;

		PhotonNetwork.InstantiateRoomObject("Enemy", spawnPos, Quaternion.identity, 0,
			new object[]
			{
				pokeData.PokeNumber,	// 도감 번호
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
