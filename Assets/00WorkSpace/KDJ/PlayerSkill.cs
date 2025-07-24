using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("��ų: ����")]
    public float dashCooldown = 5f;// ���� ��ų�� ���� ��� �ð�
    public float dashForce = 20f;// ���� �� �÷��̾�� ������ ��
    private bool canDash = true;// ���� ���� ��ų ��� ���� ����

    [Header("��ų: ���� ����")]
    public float explosionCooldown = 8f;// ���� ��ų�� ���� ��� �ð� 
    public GameObject explosionPrefab;// ���� ����Ʈ ������
    private bool canExplosion = true;// ���� ���� ��ų ��� ���� ����

    [Header("��ų: ��")]
    public float healCooldown = 10f;// �� ��ų�� ���� ��� �ð� 
    public int healAmount = 20;// �� �� ȸ���Ǵ� ü�·�
    private bool canHeal = true;// ���� �� ��ų ��� ���� ����

    private Rigidbody2D rb;// �÷��̾��� Rigidbody2D ������Ʈ 
    private PlayerHealth playerHealth;// �÷��̾��� ü�� ���� ��ũ��Ʈ

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
    /// ���� ��ų �ڷ�ƾ.
    /// ���� �ð� ���� ��ų�� �������� ���ϵ��� ��ٿ��� ����
    private IEnumerator Dash()
    {
        canDash = false;  // ��ų ��� �Ұ��� ����
        Vector2 dashDirection = rb.velocity.normalized; // ���� �̵� ����
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse); // ���������� ���� �༭ ����
        yield return new WaitForSeconds(dashCooldown);// ��ٿ� ���
        canDash = true;// ��ų ��� ����
    }
    /// ���� ��ų �ڷ�ƾ.
    /// ���� ����Ʈ�� �����ϰ� ��ٿ� ����.
    private IEnumerator Explosion()
    {
        canExplosion = false;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);// ���� ����Ʈ ����
        yield return new WaitForSeconds(explosionCooldown); // ��ٿ� ���
        canExplosion = true;
    }
    /// �� ��ų �ڷ�ƾ.
    /// �÷��̾� ü���� ȸ���ϰ� ��ٿ� ����.
    private IEnumerator Heal()
    {
        canHeal = false;
        playerHealth.Heal(healAmount);// �÷��̾� ü�� ȸ��
        yield return new WaitForSeconds(healCooldown);// ��ٿ� ���
        canHeal = true;
    }
}
