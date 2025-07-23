using UnityEngine;
using System;

public class ExpOrb : MonoBehaviour
{
    public float amount;
    public bool isActive;

    public event Action<ExpOrb> OnDespawned;

    public void Init(float amount)
    {
        this.amount = amount;
        isActive = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 플레이어에 닿으면 먹힘
        if (collider.CompareTag("Player"))
        {
            // 경험치 전달하는 로직 추가 가능
            Despawn();
        }
    }

    public void Despawn()
    {
        isActive = false;
        OnDespawned?.Invoke(this);
        gameObject.SetActive(false);
    }
}