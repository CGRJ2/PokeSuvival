using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public string monsterPrefabName = "Prefabs/Monster"; // Resources ������ �ִ� ���� ������ �̸�
    public Vector3 spawnPosition = Vector3.zero; // ���� ���� ��ġ
    public Quaternion spawnRotation = Quaternion.identity; // ���� ���� ȸ��
    void Awake() // ���� ������Ʈ�� ������ �� ȣ��
    {
        PhotonNetwork.PrefabPool = new MonsterPool(); // Ŀ���� Ǯ �Ҵ�
    }
    public override void OnJoinedRoom() // �뿡 �������� �� ȣ��Ǵ� �ݹ� �Լ�
    {
        Debug.Log("�뿡 ������, ���� ���� �õ�");
        SpawnMonster(); // ���� ���� �Լ� ȣ��
    }

    public void SpawnMonster() // ���� ���� �Լ�
    {
        PhotonNetwork.Instantiate(monsterPrefabName, spawnPosition, spawnRotation); // ��Ʈ��ũ�� ���� ����
    }
}
