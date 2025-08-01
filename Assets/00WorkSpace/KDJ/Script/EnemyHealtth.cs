using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;   // 적의 최대 체력
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }
    /// 데미지를 받는 함수
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name}이(가) {damage} 데미지를 받음. 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    /// 적 사망 처리
    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        Destroy(gameObject);
    }
}