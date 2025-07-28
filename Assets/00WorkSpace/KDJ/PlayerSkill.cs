using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("��ų: ����")]
    public float dashCooldown = 5f;       // ���� ��ų ���� ��� �ð�
    public float dashForce = 20f;         // ���� �� �÷��̾�� ������ ��
    private bool canDash = true;          // ���� ���� ��ų ��� ���� ����

    [Header("��ų: ����")]
    public float explosionCooldown = 8f;  // ���� ��ų ���� ��� �ð�
    public GameObject explosionPrefab;    // ���� ����Ʈ ������
    public float explosionDamage = 20f;   // ���� ������
    public float explosionRadius = 2f;    // ���� ���� �ݰ�
    private bool canExplosion = true;     // ���� ���� ��ų ��� ���� ����

    [Header("��ų: ��")]
    public float healCooldown = 10f;      // �� ��ų ���� ��� �ð�
    public int healAmount = 20;           // �� �� ȸ���Ǵ� ü�·�
    private bool canHeal = true;          // ���� �� ��ų ��� ���� ����

    private Rigidbody2D rb;               // �÷��̾��� Rigidbody2D ������Ʈ
    private PlayerHealth playerHealth;    // �÷��̾��� ü�� ���� ��ũ��Ʈ

    void Start()
    {
        // ���� �� �ʿ��� ������Ʈ�� ������
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Space Ű�� ������ ���� ��ų ���
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
            StartCoroutine(Dash());

        // E Ű�� ������ ���� ��ų ���
        if (Input.GetKeyDown(KeyCode.E) && canExplosion)
            StartCoroutine(Explosion());

        // Q Ű�� ������ �� ��ų ���
        if (Input.GetKeyDown(KeyCode.Q) && canHeal)
            StartCoroutine(Heal());
    }

    /// <summary>
    /// ���� ��ų �ڷ�ƾ.
    /// ���� �ð� ���� ��ų�� �������� ���ϵ��� ��ٿ��� ����
    /// </summary>
    private IEnumerator Dash()
    {
        canDash = false;  // ��ų ��� �Ұ��� ����
        Vector2 dashDirection = rb.velocity.normalized; // ���� �̵� ����
        if (dashDirection == Vector2.zero) dashDirection = Vector2.up; // ���� ���¶�� ���� ����
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse); // ���������� ���� �༭ ����
        yield return new WaitForSeconds(dashCooldown);// ��ٿ� ���
        canDash = true;// ��ų ��� ����
    }

    /// <summary>
    /// ���� ��ų �ڷ�ƾ.
    /// ���� ����Ʈ�� �����ϰ� �ֺ� ���鿡�� �������� ��.
    /// </summary>
    private IEnumerator Explosion()
    {
        canExplosion = false;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);// ���� ����Ʈ ����

        // ���� ������ �ִ� ���鿡�� ������ ����
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // ����: EnemyHealth ��ũ��Ʈ�� ���� ���� �ִٸ� ������ ����
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
        }

        yield return new WaitForSeconds(explosionCooldown); // ��ٿ� ���
        canExplosion = true;
    }

    /// <summary>
    /// �� ��ų �ڷ�ƾ.
    /// �÷��̾� ü���� ȸ���ϰ� ��ٿ� ����.
    /// </summary>
    private IEnumerator Heal()
    {
        canHeal = false;
        playerHealth.Heal(healAmount);// �÷��̾� ü�� ȸ��
        yield return new WaitForSeconds(healCooldown);// ��ٿ� ���
        canHeal = true;
    }

    /// <summary>
    /// ��ų ��ȭ �Լ� (������ �� ȣ�� ����)
    /// </summary>
    /// <param name="dashBoost">������ ������</param>
    /// <param name="explosionBoost">���� ������ ������</param>
    /// <param name="healBoost">���� ������</param>
    public void UpgradeSkills(float dashBoost, float explosionBoost, int healBoost)
    {
        dashForce += dashBoost;                 // ���� �ӵ� ��ȭ
        explosionDamage += explosionBoost;      // ���� ������ ��ȭ
        healAmount += healBoost;                // �� ȸ���� ��ȭ

        // ��ٿ��� ���� �ٿ��� ��ų�� �� ���� ��� ����
        dashCooldown = Mathf.Max(1f, dashCooldown - 0.5f);
        explosionCooldown = Mathf.Max(2f, explosionCooldown - 0.5f);
        healCooldown = Mathf.Max(3f, healCooldown - 0.5f);

        Debug.Log($"��ų ��ȭ��! ����:{dashForce}, ����:{explosionDamage}, ��:{healAmount}");
    }
}
