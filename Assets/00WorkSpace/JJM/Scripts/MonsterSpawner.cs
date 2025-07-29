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
        //int currentCount = GameObject.FindGameObjectsWithTag("Monster").Length; // ���� ���� ��(�±� �ʿ�)
        //int toSpawn = monsterCount - currentCount; // �����ؾ� �� ���� �� ���
        // Ȱ��ȭ��(���� ������ �����ϴ�) ���͸� ī��Ʈ
        int currentCount = 0;
        var monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var m in monsters)
        {
            if (m.activeSelf) currentCount++;
        }

        int toSpawn = monsterCount - currentCount;

        for (int i = 0; i < toSpawn; i++) // ������ ��ŭ�� ����
        {
            float x = Random.Range(spawnMin.x, spawnMax.x); // x�� ���� ��ġ
            float z = Random.Range(spawnMin.y, spawnMax.y); // z�� ���� ��ġ
            Vector3 pos = new Vector3(x, 0, z); // y�� 0���� ����

            PhotonNetwork.Instantiate(monsterPrefabName, pos, spawnRotation); // ���� ����
        }
    }
}
