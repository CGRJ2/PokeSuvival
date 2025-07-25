using UnityEngine;
using Photon.Pun;

public class MonsterBallSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY = 50f;
    [SerializeField] private float spawnHeight = 1f; // 바닥 위 높이

    private float spawnTimer;

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        // 마스터 클라이언트만 스폰 로직 실행
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
        // 랜덤 위치 생성 (-50,50)
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(randomX, randomY, spawnHeight); // 2D 게임은 Z=0

        // 풀 매니저에 스폰 요청
        MonsterBallPoolManager.Instance.SpawnMonsterBall(spawnPosition);
    }
}
