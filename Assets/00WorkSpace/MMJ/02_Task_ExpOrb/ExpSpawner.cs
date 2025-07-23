using System.Collections.Generic;
using UnityEngine;

public class ExpOrbSpawner : MonoBehaviour
{
    [SerializeField] private ExpOrbPool orbPool;
    [SerializeField] private int maxOrbs = 50;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnAmountPerInterval = 5;
    [SerializeField] private Vector2 mapSize = new Vector2(100f, 100f);

    private List<ExpOrb> activeOrbs = new List<ExpOrb>();

    private void Start()
    {
        SpawnOrbsImmediate(); // 처음에 한방에 뿌리게 하는 디버그용

        InvokeRepeating(nameof(SpawnOrbs), 1f, spawnInterval);
    }

    private void SpawnOrbsImmediate()
    {
        int spawnCount = maxOrbs;

        for (int i = 0; i < spawnCount; i++)
        {
            ExpOrb orb = orbPool.GetOrb();
            orb.transform.position = GetRandomPosition();
            orb.Init(Random.Range(1f, 3f));
            orb.gameObject.SetActive(true);
            activeOrbs.Add(orb);

            // 중복 구독 방지
            orb.OnDespawned -= HandleOrbDespawned;
            orb.OnDespawned += HandleOrbDespawned;
        }
    }

    private void SpawnOrbs()
    {
        int currentCount = activeOrbs.Count;

        if (currentCount >= maxOrbs)
            return;

        int spawnCount = Mathf.Min(spawnAmountPerInterval, maxOrbs - currentCount);

        for (int i = 0; i < spawnCount; i++)
        {
            ExpOrb orb = orbPool.GetOrb();
            orb.transform.position = GetRandomPosition();
            orb.Init(Random.Range(1f, 3f));
            orb.gameObject.SetActive(true);
            activeOrbs.Add(orb);
            orb.OnDespawned += HandleOrbDespawned;
        }
    }

    private Vector2 GetRandomPosition()
    {
        float x = Random.Range(-mapSize.x / 2f, mapSize.x / 2f);
        float y = Random.Range(-mapSize.y / 2f, mapSize.y / 2f);
        return new Vector2(x, y);
    }

    private void HandleOrbDespawned(ExpOrb orb)
    {
        if (activeOrbs.Contains(orb))
        {
            activeOrbs.Remove(orb);
            orb.OnDespawned -= HandleOrbDespawned;
            orbPool.ReturnOrb(orb); // 추가로 반환까지
        }
    }
}