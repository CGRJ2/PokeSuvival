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
        // �÷��̾ ������ ����
        if (collider.CompareTag("Player"))
        {
            // ����ġ �����ϴ� ���� �߰� ����
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