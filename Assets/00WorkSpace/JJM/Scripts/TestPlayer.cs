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

    // �÷��̾��� BattleDataTable ���� ��ȯ 
    public BattleDataTable BattleData
    {
        get
        {
            // ���� ���� ������ ä���� ��ȯ�ؾ� ��
            return new BattleDataTable(
                level: 1,
                pokeData: null,
                pokeStat: new PokemonStat { Attak = attackDamage, Hp = maxHealth },
                maxHp: maxHealth,
                currentHp: currentHealth
            );
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 8방향 이동 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // 공격 (스페이스바)
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
        // 플레이어 주변에 있는 몬스터 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                IDamagable monster = hit.GetComponent<IDamagable>();
                if (monster != null)
                {
                    monster.TakeDamage(this.BattleData, null);
                }
            }
        }
    }

    // IDamagable �������̽� ����
    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        int damage = 0;
        if (skill != null)
            damage = skill.Damage;
        else
            damage = attackerData.AllStat.Attak; // �⺻ ���ݷ�

        currentHealth -= damage;
        Debug.Log($"�÷��̾ ������ ����: {damage} ���� ü��: {currentHealth}");
        if (currentHealth <= 0)
        {
            return true;
            Debug.Log("플레이어가 데미지 받음: " + damage + " 남은 체력: " + currentHealth);
        }
        return false;
    }

    // 시각적 디버그용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

	public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
	{
		throw new System.NotImplementedException();
	}
}
