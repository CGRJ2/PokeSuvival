using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrbPool : MonoBehaviour
{
    public static ExpOrbPool Instance { get; private set; }

    [SerializeField] private ExpOrb orbPrefab;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private int initialPoolSize = 50;

    private Queue<ExpOrb> pool = new Queue<ExpOrb>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitPool(initialPoolSize);
    }

    public void InitPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ExpOrb orb = Instantiate(orbPrefab, parentTransform);
            orb.gameObject.SetActive(false);
            pool.Enqueue(orb);
        }
    }

    public ExpOrb GetOrb()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        // 老窜~ 何练窍搁 货肺 积己
        ExpOrb orb = Instantiate(orbPrefab, parentTransform);
        orb.gameObject.SetActive(false);
        return orb;
    }

    public void ReturnOrb(ExpOrb orb)
    {
        orb.gameObject.SetActive(false);
        pool.Enqueue(orb);
    }
}
