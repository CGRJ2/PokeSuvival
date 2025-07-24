using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>(); // �����պ� Ǯ ����

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // ������Ʈ ���� ��û �� ȣ��
    {
        if (!pool.ContainsKey(prefabId)) // �ش� ������ Ǯ ������
            pool[prefabId] = new Queue<GameObject>(); // �� Ǯ ����

        if (pool[prefabId].Count > 0) // Ǯ�� ������Ʈ�� ������
        {
            GameObject obj = pool[prefabId].Dequeue(); // �ϳ� ����
            obj.transform.position = position; // ��ġ ����
            obj.transform.rotation = rotation; // ȸ�� ����
            obj.SetActive(true); // Ȱ��ȭ
            return obj; // ��ȯ
        }
        else // Ǯ�� ������
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId); // ������ �ε�
            GameObject obj = Object.Instantiate(prefab, position, rotation); // ���� ����
            return obj; // ��ȯ
        }
    }

    public void Destroy(GameObject gameObject) // ������Ʈ ���� ��û �� ȣ��
    {
        gameObject.SetActive(false); // ��Ȱ��ȭ
        pool[gameObject.name].Enqueue(gameObject); // Ǯ�� �ٽ� ����
    }
}
