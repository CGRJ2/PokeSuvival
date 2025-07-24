using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [SerializeField] private float mapWidth = 100f;
    [SerializeField] private float mapHeight = 100f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Rect GetMapBounds()
    {
        return new Rect(-mapWidth / 2f, -mapHeight / 2f, mapWidth, mapHeight);
    }

    public Vector2 GetRandomPosition()
    {
        float x = Random.Range(-mapWidth / 2f, mapWidth / 2f);
        float y = Random.Range(-mapHeight / 2f, mapHeight / 2f);
        return new Vector2(x, y);
    }

}
