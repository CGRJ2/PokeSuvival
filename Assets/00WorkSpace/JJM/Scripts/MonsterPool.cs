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

        if (pool[prefabId].Count > 0) // Ǯ�� ������Ʈ�� ������
        {
            GameObject obj = pool[prefabId].Dequeue(); // �ϳ� ����
            obj.transform.position = position; // ��ġ ����
            obj.transform.rotation = rotation; // ȸ�� ����
            return obj; // ��ȯ
        }
        else // Ǯ�� ������
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId); // ������ �ε�
            GameObject obj = Object.Instantiate(prefab, position, rotation); // ���� ����
            obj.SetActive(false); // ���� ������ ���� ��Ȱ��ȭ�� ��ȯ
            objectToPrefabId[obj] = prefabId; // ������Ʈ�� prefabId ���� ����
            return obj; // ��ȯ
        }
    }

    public void Destroy(GameObject gameObject) // ������Ʈ ���� ��û �� ȣ��
    {
        gameObject.SetActive(false); // ��Ȱ��ȭ

        string prefabId;
        // ������Ʈ�� �ش��ϴ� prefabId�� ã��
        if (!objectToPrefabId.TryGetValue(gameObject, out prefabId))
        {
            // �� ã���� name���� (Clone) �����ؼ� �õ�
            prefabId = gameObject.name.Replace("(Clone)", "").Trim();
        }

        if (!pool.ContainsKey(prefabId)) // �ش� ������ Ǯ ������
            pool[prefabId] = new Queue<GameObject>(); // �� Ǯ ����

        pool[gameObject.name].Enqueue(gameObject); // Ǯ�� �ٽ� ����
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
