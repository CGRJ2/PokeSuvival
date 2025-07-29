using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public string monsterPrefabName = "Prefabs/Monster"; // Resources ������ �ִ� ���� ������ �̸�
    [SerializeField] public int monsterCount = 5; // ������ ���� ��
    public Vector2 spawnMin = new Vector2(-10, -10); // ���� ���� �ּ� ��ǥ(x, z)
    public Vector2 spawnMax = new Vector2(10, 10);   // ���� ���� �ִ� ��ǥ(x, z)
    public Quaternion spawnRotation = Quaternion.identity; // ���� ���� ȸ����

    void Awake() // ���� ������Ʈ�� ������ �� ȣ��
    {
        PhotonNetwork.PrefabPool = new MonsterPool(); // Ŀ���� Ǯ �Ҵ�
    }

    public override void OnJoinedRoom() // �뿡 �������� �� ȣ��Ǵ� �ݹ� �Լ�
    {
        Debug.Log("�뿡 ������, ���� ���� �õ�"); // ����� �α� ���
        TrySpawnMonsters(); // ���� ���� �õ�
    }
    void Update() // �� �����Ӹ��� ȣ��
    {
        if (PhotonNetwork.IsMasterClient) // ������ Ŭ���̾�Ʈ�� ����
        {
            TrySpawnMonsters(); // �׻� ���� ����
        }
    }


    void TrySpawnMonsters() // ���� ���� ���� �Լ�
    {
        int currentCount = 0; // ���� Ȱ��ȭ�� ���� ���� ������ ����
        var monsters = GameObject.FindGameObjectsWithTag("Monster"); // "Monster" �±׸� ���� ��� ������Ʈ �迭 ��������
        foreach (var m in monsters) // �迭�� ��ȸ
        {
            if (m.activeSelf) currentCount++; // Ȱ��ȭ�� ������Ʈ�� ī��Ʈ
        }

        int toSpawn = monsterCount - currentCount; // �����ؾ� �� ���� �� ���
        float minDistance = 1.0f; // ���� �� �ּ� �Ÿ�

        for (int i = 0; i < toSpawn; i++) // ������ ��ŭ�� �ݺ�
        {
            Vector3 spawnPos = Vector3.zero; // ���� ���� ��ġ�� ������ ����
            bool found = false; // ������ ��ġ�� ã�Ҵ��� ����
            int tryCount = 0; // �õ� Ƚ��
            int maxTry = 30; // �ִ� �õ� Ƚ��(���ѷ��� ����)

            while (!found && tryCount < maxTry) // ������ ��ġ�� ã�ų� �ִ� �õ����� �ݺ�
            {
                float x = Random.Range(spawnMin.x, spawnMax.x); // x�� ���� ��ġ
                float z = Random.Range(spawnMin.y, spawnMax.y); // z�� ���� ��ġ
                spawnPos = new Vector3(x, 0, z); // y�� 0���� ����

                bool overlap = false; // ��ħ ���� �÷��� �ʱ�ȭ

                foreach (var m in monsters) // ���� ���͵�� �Ÿ� ��
                {
                    if (!m.activeSelf) continue; // ��Ȱ��ȭ�� ������Ʈ�� ����
                    if (Vector3.Distance(m.transform.position, spawnPos) < minDistance) // �ּ� �Ÿ� �̸��̸�
                    {
                        overlap = true; // ��ħ �߻�
                        break; // �� �̻� �˻����� �ʰ� �ߴ�
                    }
                }

                if (!overlap) // ��ġ�� ������
                {
                    found = true; // ��ġ ã��
                }
                tryCount++; // �õ� Ƚ�� ����
            }

            if (found) // ������ ��ġ�� ã������
            {
                PhotonNetwork.Instantiate(monsterPrefabName, spawnPos, spawnRotation); // ���� ����
            }
            // �� ã������ �������� ����(���ѷ��� ����)
        }
    }
}
