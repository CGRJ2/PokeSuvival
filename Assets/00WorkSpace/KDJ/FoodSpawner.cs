using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;   // ���� Prefab
    public float spawnInterval = 2f; // ���� �ֱ� (��)
    public Vector2 spawnRange = new Vector2(10f, 10f); // ���� ����

    void Start()
    {
        InvokeRepeating("SpawnFood", 0f, spawnInterval);
    }

    void SpawnFood()
    {
        Vector2 spawnPos = new Vector2(Random.Range(-spawnRange.x, spawnRange.x),
                                       Random.Range(-spawnRange.y, spawnRange.y));
        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }
}
