using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>(); // �����պ� Ǯ ����

    // �� ������Ʈ�� prefabId�� �����ϱ� ���� ��ųʸ�
    private Dictionary<GameObject, string> objectToPrefabId = new Dictionary<GameObject, string>(); // ������Ʈ�� prefabId ����

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // ������Ʈ ���� ��û �� ȣ��
    {
        if (!pool.ContainsKey(prefabId)) // �ش� ������ Ǯ ������
            pool[prefabId] = new Queue<GameObject>(); // �� Ǯ ����

        GameObject obj; // ��ȯ�� ������Ʈ ���� ����

        if (pool[prefabId].Count > 0) // Ǯ�� ������Ʈ�� ������
        {
            obj = pool[prefabId].Dequeue(); // �ϳ� ����
            obj.transform.position = position; // ��ġ ����
            obj.transform.rotation = rotation; // ȸ�� ����
        }
        else // Ǯ�� ������
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId); // ������ �ε�
            obj = Object.Instantiate(prefab, position, rotation); // ���� ����
        }

        objectToPrefabId[obj] = prefabId; // ������Ʈ�� prefabId ���� ����(�ߺ� ��� ����)

        // === ���⼭ ���� �ʱ�ȭ ===
        Monster monster = obj.GetComponent<Monster>(); // Monster ������Ʈ ��������
        if (monster != null) monster.ResetMonster(); // ���� �ʱ�ȭ �Լ� ȣ��(������, ü�� �� ���󺹱�)
        // =======================

        obj.SetActive(false); // ���� ������ ���� ��Ȱ��ȭ�� ��ȯ
        return obj; // ��ȯ


    }

    public void Destroy(GameObject gameObject) // ������Ʈ ���� ��û �� ȣ��
    {
        gameObject.SetActive(false); // ��Ȱ��ȭ

        string prefabId;
        if (!objectToPrefabId.TryGetValue(gameObject, out prefabId)) // ������ID�� ��ųʸ����� �����ϰ� ã��
        {
            Debug.LogError($"MonsterPool: objectToPrefabId�� {gameObject.name}��(��) �����ϴ�.");
            return;
        }

        if (!pool.ContainsKey(prefabId)) // �ش� ������ Ǯ ������
            pool[prefabId] = new Queue<GameObject>(); // �� Ǯ ����

        pool[prefabId].Enqueue(gameObject); // prefabId�� Ǯ�� �ٽ� ����
    }
}
//PhotonNetwork.PrefabPool�� �Ҵ�
/*
 * void Awake() // ���� ������Ʈ�� ������ �� ȣ��
{
    PhotonNetwork.PrefabPool = new MonsterPool(); // Ŀ���� Ǯ �Ҵ�
}
PhotonNetwork.Instantiate("MonsterPrefab", position, rotation); // ����
PhotonNetwork.Destroy(monsterObject); // ����(Ǯ�� ��ȯ)
 * */
