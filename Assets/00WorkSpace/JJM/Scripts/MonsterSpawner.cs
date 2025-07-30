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
        int currentCount = 0; // 현재 활성화된 몬스터 수를 저장할 변수
        var monsters = GameObject.FindGameObjectsWithTag("Monster"); // "Monster" 태그를 가진 모든 오브젝트 배열 가져오기
        foreach (var m in monsters) // 배열을 순회
        {
            if (m.activeSelf) currentCount++; // 활성화된 오브젝트만 카운트
        }

        int toSpawn = monsterCount - currentCount; // 생성해야 할 몬스터 수 계산
        float minDistance = 1.0f; // 몬스터 간 최소 거리

        for (int i = 0; i < toSpawn; i++) // 부족한 만큼만 반복
        {
            Vector3 spawnPos = Vector3.zero; // 실제 생성 위치를 저장할 변수
            bool found = false; // 적절한 위치를 찾았는지 여부
            int tryCount = 0; // 시도 횟수
            int maxTry = 30; // 최대 시도 횟수(무한루프 방지)

            while (!found && tryCount < maxTry) // 적절한 위치를 찾거나 최대 시도까지 반복
            {
                float x = Random.Range(spawnMin.x, spawnMax.x); // x축 랜덤 위치
                float z = Random.Range(spawnMin.y, spawnMax.y); // z축 랜덤 위치
                spawnPos = new Vector3(x, 0, z); // y는 0으로 고정

                bool overlap = false; // 겹침 여부 플래그 초기화

                foreach (var m in monsters) // 기존 몬스터들과 거리 비교
                {
                    if (!m.activeSelf) continue; // 비활성화된 오브젝트는 무시
                    if (Vector3.Distance(m.transform.position, spawnPos) < minDistance) // 최소 거리 미만이면
                    {
                        overlap = true; // 겹침 발생
                        break; // 더 이상 검사하지 않고 중단
                    }
                }

                if (!overlap) // 겹치지 않으면
                {
                    found = true; // 위치 찾음
                }
                tryCount++; // 시도 횟수 증가
            }

            if (found) // 적절한 위치를 찾았으면
            {
                PhotonNetwork.Instantiate(monsterPrefabName, spawnPos, spawnRotation); // 몬스터 생성
            }
            // 못 찾았으면 생성하지 않음(무한루프 방지)
        }
    }
}
