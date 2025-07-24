using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>(); // 프리팹별 풀 저장

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // 오브젝트 생성 요청 시 호출
    {
        if (!pool.ContainsKey(prefabId)) // 해당 프리팹 풀 없으면
            pool[prefabId] = new Queue<GameObject>(); // 새 풀 생성

        if (pool[prefabId].Count > 0) // 풀에 오브젝트가 있으면
        {
            GameObject obj = pool[prefabId].Dequeue(); // 하나 꺼냄
            obj.transform.position = position; // 위치 설정
            obj.transform.rotation = rotation; // 회전 설정
            obj.SetActive(true); // 활성화
            return obj; // 반환
        }
        else // 풀에 없으면
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId); // 프리팹 로드
            GameObject obj = Object.Instantiate(prefab, position, rotation); // 새로 생성
            return obj; // 반환
        }
    }

    public void Destroy(GameObject gameObject) // 오브젝트 삭제 요청 시 호출
    {
        gameObject.SetActive(false); // 비활성화
        pool[gameObject.name].Enqueue(gameObject); // 풀에 다시 넣음
    }
}
