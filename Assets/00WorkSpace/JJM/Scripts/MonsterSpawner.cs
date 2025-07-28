using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public string monsterPrefabName = "Prefabs/Monster"; // Resources 폴더에 있는 몬스터 프리팹 이름
    [SerializeField] public int monsterCount = 5; // 유지할 몬스터 수
    public Vector2 spawnMin = new Vector2(-10, -10); // 랜덤 생성 최소 좌표(x, z)
    public Vector2 spawnMax = new Vector2(10, 10);   // 랜덤 생성 최대 좌표(x, z)
    public Quaternion spawnRotation = Quaternion.identity; // 몬스터 생성 회전값

    void Awake() // 게임 오브젝트가 생성될 때 호출
    {
        PhotonNetwork.PrefabPool = new MonsterPool(); // 커스텀 풀 할당
    }

    public override void OnJoinedRoom() // 룸에 입장했을 때 호출되는 콜백 함수
    {
        Debug.Log("룸에 입장함, 몬스터 생성 시도"); // 디버그 로그 출력
        TrySpawnMonsters(); // 몬스터 생성 시도
    }
    void Update() // 매 프레임마다 호출
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트만 관리
        {
            TrySpawnMonsters(); // 항상 개수 유지
        }
    }


    void TrySpawnMonsters() // 몬스터 개수 유지 함수
    {
        //int currentCount = GameObject.FindGameObjectsWithTag("Monster").Length; // 현재 몬스터 수(태그 필요)
        //int toSpawn = monsterCount - currentCount; // 생성해야 할 몬스터 수 계산
        // 활성화된(씬에 실제로 존재하는) 몬스터만 카운트
        int currentCount = 0;
        var monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var m in monsters)
        {
            if (m.activeSelf) currentCount++;
        }

        int toSpawn = monsterCount - currentCount;

        for (int i = 0; i < toSpawn; i++) // 부족한 만큼만 생성
        {
            float x = Random.Range(spawnMin.x, spawnMax.x); // x축 랜덤 위치
            float z = Random.Range(spawnMin.y, spawnMax.y); // z축 랜덤 위치
            Vector3 pos = new Vector3(x, 0, z); // y는 0으로 고정

            PhotonNetwork.Instantiate(monsterPrefabName, pos, spawnRotation); // 몬스터 생성
        }
    }
}
