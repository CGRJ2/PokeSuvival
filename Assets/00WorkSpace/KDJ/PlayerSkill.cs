using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("스킬: 돌진")]
    public float dashCooldown = 5f;       // 돌진 스킬 재사용 대기 시간
    public float dashForce = 20f;         // 돌진 시 플레이어에게 적용할 힘
    private bool canDash = true;          // 현재 돌진 스킬 사용 가능 여부

    [Header("스킬: 폭발")]
    public float explosionCooldown = 8f;  // 폭발 스킬 재사용 대기 시간
    public GameObject explosionPrefab;    // 폭발 이펙트 프리팹
    public float explosionDamage = 20f;   // 폭발 데미지
    public float explosionRadius = 2f;    // 폭발 범위 반경
    private bool canExplosion = true;     // 현재 폭발 스킬 사용 가능 여부

    [Header("스킬: 힐")]
    public float healCooldown = 10f;      // 힐 스킬 재사용 대기 시간
    public int healAmount = 20;           // 힐 시 회복되는 체력량
    private bool canHeal = true;          // 현재 힐 스킬 사용 가능 여부

    private Rigidbody2D rb;               // 플레이어의 Rigidbody2D 컴포넌트
    private PlayerHealth playerHealth;    // 플레이어의 체력 관리 스크립트

    void Start()
    {
        // 시작 시 필요한 컴포넌트를 가져옴
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Space 키를 누르면 돌진 스킬 사용
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
            StartCoroutine(Dash());

        // E 키를 누르면 폭발 스킬 사용
        if (Input.GetKeyDown(KeyCode.E) && canExplosion)
            StartCoroutine(Explosion());

        // Q 키를 누르면 힐 스킬 사용
        if (Input.GetKeyDown(KeyCode.Q) && canHeal)
            StartCoroutine(Heal());
    }

    /// <summary>
    /// 돌진 스킬 코루틴.
    /// 일정 시간 동안 스킬을 재사용하지 못하도록 쿨다운을 적용
    /// </summary>
    private IEnumerator Dash()
    {
        canDash = false;  // 스킬 사용 불가로 설정
        Vector2 dashDirection = rb.velocity.normalized; // 현재 이동 방향
        if (dashDirection == Vector2.zero) dashDirection = Vector2.up; // 멈춘 상태라면 위로 돌진
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse); // 순간적으로 힘을 줘서 돌진
        yield return new WaitForSeconds(dashCooldown);// 쿨다운 대기
        canDash = true;// 스킬 사용 가능
    }

    /// <summary>
    /// 폭발 스킬 코루틴.
    /// 폭발 이펙트를 생성하고 주변 적들에게 데미지를 줌.
    /// </summary>
    private IEnumerator Explosion()
    {
        canExplosion = false;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);// 폭발 이펙트 생성

        // 폭발 범위에 있는 적들에게 데미지 전달
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // 예시: EnemyHealth 스크립트를 가진 적이 있다면 데미지 적용
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
        }

        yield return new WaitForSeconds(explosionCooldown); // 쿨다운 대기
        canExplosion = true;
    }

    /// <summary>
    /// 힐 스킬 코루틴.
    /// 플레이어 체력을 회복하고 쿨다운 적용.
    /// </summary>
    private IEnumerator Heal()
    {
        canHeal = false;
        playerHealth.Heal(healAmount);// 플레이어 체력 회복
        yield return new WaitForSeconds(healCooldown);// 쿨다운 대기
        canHeal = true;
    }

    /// <summary>
    /// 스킬 강화 함수 (레벨업 시 호출 가능)
    /// </summary>
    /// <param name="dashBoost">돌진력 증가량</param>
    /// <param name="explosionBoost">폭발 데미지 증가량</param>
    /// <param name="healBoost">힐량 증가량</param>
    public void UpgradeSkills(float dashBoost, float explosionBoost, int healBoost)
    {
        dashForce += dashBoost;                 // 돌진 속도 강화
        explosionDamage += explosionBoost;      // 폭발 데미지 강화
        healAmount += healBoost;                // 힐 회복량 강화

        // 쿨다운을 소폭 줄여서 스킬을 더 자주 사용 가능
        dashCooldown = Mathf.Max(1f, dashCooldown - 0.5f);
        explosionCooldown = Mathf.Max(2f, explosionCooldown - 0.5f);
        healCooldown = Mathf.Max(3f, healCooldown - 0.5f);

        Debug.Log($"스킬 강화됨! 돌진:{dashForce}, 폭발:{explosionDamage}, 힐:{healAmount}");
    }
}
