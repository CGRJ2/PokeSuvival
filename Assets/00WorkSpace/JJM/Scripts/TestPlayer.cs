using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour, IDamagable

{
    public float moveSpeed = 5f;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int attackDamage = 20;
    public float attackRange = 1.5f;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    public BattleDataTable BattleData => throw new System.NotImplementedException();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 8���� �̵� �Է�
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // ���� (�����̽���)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        // �÷��̾� �ֺ��� �ִ� ���� Ž��
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                IDamagable monster = hit.GetComponent<IDamagable>();
                if (monster != null)
                {
                    monster.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("�÷��̾ ������ ����: " + damage + " ���� ü��: " + currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("�÷��̾� ���");
            // ��� ó��
        }
    }

    // �ð��� ����׿�
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
