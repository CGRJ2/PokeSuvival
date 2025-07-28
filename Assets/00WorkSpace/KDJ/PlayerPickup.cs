using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerPickup : MonoBehaviour
{
    private PlayerEvolution evolution;

    void Start()
    {
        evolution = GetComponent<PlayerEvolution>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Food 태그를 가진 아이템과 충돌 시 경험치 증가
        if (collision.CompareTag("Food"))
        {
            evolution.AddExperience(1);
            Destroy(collision.gameObject);
        }
    }
}
