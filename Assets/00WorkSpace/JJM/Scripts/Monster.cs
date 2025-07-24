using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float detectRange = 10f;      // �÷��̾ ������ �� �ִ� �Ÿ�
    public float moveSpeed = 3f;         // ������ �̵� �ӵ�
    public float attackRange = 2f;       // �÷��̾ ������ �� �ִ� �Ÿ�
    public float attackCooldown = 1.5f;  // ���� �� �ٽ� ������ �������� ��� �ð�
    public int maxHealth = 100;          // ������ �ִ� ü��

    private int currentHealth;           // ���� ü��
    private Transform player;            // �÷��̾��� Transform ����
    private float lastAttackTime;        // ������ ���� �ð�

    private Vector3 wanderDirection; // ���� �̵� ������ �����ϴ� ����
    private float wanderTimer; // ������ �ٲ� ������ ���� �ð��� �����ϴ� ����
    public float wanderChangeInterval = 2f; // ������ �ٲ� �ð� ����(��)

    void Start() // ���� ���� �� ȣ��Ǵ� �Լ�
    {
        currentHealth = maxHealth; // ���� ü���� �ִ� ü������ �ʱ�ȭ
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // "Player" �±׸� ���� ������Ʈ�� Transform�� ã��
        SetRandomWanderDirection(); // ������ �� ���� ������ �� �� ����
    }

    void Update() // �� �����Ӹ��� ȣ��Ǵ� �Լ�
    {
        if (player == null) return; // �÷��̾ ������ �ƹ��͵� ���� ����

        float distance = Vector3.Distance(transform.position, player.position); // ���Ϳ� �÷��̾� ������ �Ÿ� ���

        if (distance <= detectRange) // �÷��̾ ���� ���� ���� ���� ��
        {
            MoveTowards(player.position); // �÷��̾ ���� �̵�

            if (distance <= attackRange && Time.time - lastAttackTime > attackCooldown) // ���� ���� ���̰� ��Ÿ���� ������ ��
            {
                AttackPlayer(); // �÷��̾� ����
                lastAttackTime = Time.time; // ������ ���� �ð� ����
            }
        }
        else // �÷��̾ ���� ���� �ۿ� ���� ��
        {
            Wander(); // �����Ӱ� �̵�
        }
    }

    void MoveTowards(Vector3 target) // ������ ��ġ�� �̵��ϴ� �Լ�
    {
        Vector3 direction = (target - transform.position).normalized; // ��ǥ ��ġ������ ���� ���� ��� �� ����ȭ
        transform.position += direction * moveSpeed * Time.deltaTime; // �ش� �������� �̵�
        // �ʿ�� ȸ�� �߰�
    }

    void Wander() // ���� �̵�(���� �̵� ��) �Լ�
    {
        wanderTimer -= Time.deltaTime; // Ÿ�̸Ӹ� �� �����Ӹ��� ���ҽ�Ŵ
        if (wanderTimer <= 0f) // Ÿ�̸Ӱ� 0 ���ϰ� �Ǹ�
        {
            SetRandomWanderDirection(); // ���ο� ���� ������ ����
        }
        transform.position += wanderDirection * moveSpeed * Time.deltaTime; // ���� �������� �̵�
    }

    // ���� ������ �����ϴ� �Լ�
    void SetRandomWanderDirection() // ���ο� ���� ������ �����ϴ� �Լ�
    {
        float angle = Random.Range(0f, 360f); // 0~360�� �߿��� ���� ������ ����
        wanderDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized; // ���� ���� ���� ���� ���
        wanderTimer = wanderChangeInterval; // Ÿ�̸Ӹ� �ٽ� �ʱ�ȭ
    }

    void AttackPlayer() // �÷��̾ �����ϴ� �Լ�
    {
        // �÷��̾�� ������ �ֱ�
        // ����: player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

    public void TakeDamage(int damage) // ���Ͱ� �������� �޴� �Լ�
    {
        currentHealth -= damage; // ���� ��������ŭ ü�� ����
        if (currentHealth <= 0) // ü���� 0 ���ϰ� �Ǹ�
        {
            Die(); // ��� ó��
        }
    }

    void Die() // ���Ͱ� �׾��� �� ȣ��Ǵ� �Լ�
    {
        // ����ġ ���� ���� ��ġ
        // TODO: ����ġ ���� ������ ���� �� ���
        // ����: Instantiate(expOrbPrefab, transform.position, Quaternion.identity);

        // ������ ��� ��ġ
        // TODO: ������ ������ ���� �� ���
        // ����: Instantiate(itemPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject); // ���� ������Ʈ ����
    }
}
