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

    // 플레이어의 BattleDataTable 정보 반환 
    public BattleDataTable BattleData
    {
        get
        {
            // 실제 스탯 정보를 채워서 반환해야 함
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
                    // 기본 공격: BattleDataTable과 null(스킬 없음) 전달
                    monster.TakeDamage(this.BattleData, null);
                }
            }
        }
    }

    // IDamagable 인터페이스 구현
    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        int damage = 0;
        if (skill != null)
            damage = skill.Damage;
        else
            damage = attackerData.AllStat.Attak; // 기본 공격력

        currentHealth -= damage;
        Debug.Log($"플레이어가 데미지 받음: {damage} 남은 체력: {currentHealth}");
        if (currentHealth <= 0)
        {
            Debug.Log("플레이어 사망");
           
            // 사망 처리
            return true;
        }
        return false;
    }

    // 시각적 디버그용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
