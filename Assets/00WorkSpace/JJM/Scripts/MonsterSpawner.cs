using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public string monsterPrefabName = "Prefabs/Monster"; // Resources 폴더에 있는 몬스터 프리팹 이름
    public Vector3 spawnPosition = Vector3.zero; // 몬스터 생성 위치
    public Quaternion spawnRotation = Quaternion.identity; // 몬스터 생성 회전
    void Awake() // 게임 오브젝트가 생성될 때 호출
    {
        PhotonNetwork.PrefabPool = new MonsterPool(); // 커스텀 풀 할당
    }
    public override void OnJoinedRoom() // 룸에 입장했을 때 호출되는 콜백 함수
    {
        Debug.Log("룸에 입장함, 몬스터 생성 시도");
        SpawnMonster(); // 몬스터 생성 함수 호출
    }

    public void SpawnMonster() // 몬스터 생성 함수
    {
        PhotonNetwork.Instantiate(monsterPrefabName, spawnPosition, spawnRotation); // 네트워크로 몬스터 생성
    }
}
