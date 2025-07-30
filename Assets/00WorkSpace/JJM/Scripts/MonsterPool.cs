using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>(); // 프리팹별 풀 저장

    // 각 오브젝트에 prefabId를 저장하기 위한 딕셔너리
    private Dictionary<GameObject, string> objectToPrefabId = new Dictionary<GameObject, string>(); // 오브젝트별 prefabId 매핑

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // 오브젝트 생성 요청 시 호출
    {
        if (!pool.ContainsKey(prefabId)) // 해당 프리팹 풀 없으면
            pool[prefabId] = new Queue<GameObject>(); // 새 풀 생성

        GameObject obj; // 반환할 오브젝트 변수 선언

        if (pool[prefabId].Count > 0) // 풀에 오브젝트가 있으면
        {
            obj = pool[prefabId].Dequeue(); // 하나 꺼냄
            obj.transform.position = position; // 위치 설정
            obj.transform.rotation = rotation; // 회전 설정
        }
        else // 풀에 없으면
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId); // 프리팹 로드
            obj = Object.Instantiate(prefab, position, rotation); // 새로 생성
        }

        objectToPrefabId[obj] = prefabId; // 오브젝트와 prefabId 매핑 저장(중복 등록 안전)

        // === 여기서 상태 초기화 ===
        Monster monster = obj.GetComponent<Monster>(); // Monster 컴포넌트 가져오기
        if (monster != null) monster.ResetMonster(); // 상태 초기화 함수 호출(반투명, 체력 등 원상복구)
        // =======================

        obj.SetActive(false); // 새로 생성할 때도 비활성화로 반환
        return obj; // 반환


    }

    public void Destroy(GameObject gameObject) // 오브젝트 삭제 요청 시 호출
    {
        gameObject.SetActive(false); // 비활성화

        string prefabId;
        if (!objectToPrefabId.TryGetValue(gameObject, out prefabId)) // 프리팹ID를 딕셔너리에서 안전하게 찾기
        {
            Debug.LogError($"MonsterPool: objectToPrefabId에 {gameObject.name}이(가) 없습니다.");
            return;
        }

        if (!pool.ContainsKey(prefabId)) // 해당 프리팹 풀 없으면
            pool[prefabId] = new Queue<GameObject>(); // 새 풀 생성

        pool[prefabId].Enqueue(gameObject); // prefabId로 풀에 다시 넣음
    }
}
//PhotonNetwork.PrefabPool에 할당
/*
 * void Awake() // 게임 오브젝트가 생성될 때 호출
{
    PhotonNetwork.PrefabPool = new MonsterPool(); // 커스텀 풀 할당
}
PhotonNetwork.Instantiate("MonsterPrefab", position, rotation); // 생성
PhotonNetwork.Destroy(monsterObject); // 삭제(풀로 반환)
 * */
