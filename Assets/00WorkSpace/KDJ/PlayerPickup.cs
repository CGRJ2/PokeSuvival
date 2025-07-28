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
        // Food �±׸� ���� �����۰� �浹 �� ����ġ ����
        if (collision.CompareTag("Food"))
        {
            evolution.AddExperience(1);
            Destroy(collision.gameObject);
        }
    }
}
