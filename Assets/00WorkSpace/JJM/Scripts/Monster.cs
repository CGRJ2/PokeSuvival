using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float detectRange = 10f;      // 플레이어를 감지할 수 있는 거리
    public float moveSpeed = 3f;         // 몬스터의 이동 속도
    public float attackRange = 2f;       // 플레이어를 공격할 수 있는 거리
    public float attackCooldown = 1.5f;  // 공격 후 다시 공격할 때까지의 대기 시간
    public int maxHealth = 100;          // 몬스터의 최대 체력

    private int currentHealth;           // 현재 체력
    private Transform player;            // 플레이어의 Transform 참조
    private float lastAttackTime;        // 마지막 공격 시각

    private Vector3 wanderDirection; // 현재 이동 방향을 저장하는 변수
    private float wanderTimer; // 방향을 바꿀 때까지 남은 시간을 저장하는 변수
    public float wanderChangeInterval = 2f; // 방향을 바꿀 시간 간격(초)

    void Start() // 게임 시작 시 호출되는 함수
    {
        currentHealth = maxHealth; // 현재 체력을 최대 체력으로 초기화
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // "Player" 태그를 가진 오브젝트의 Transform을 찾음
        SetRandomWanderDirection(); // 시작할 때 랜덤 방향을 한 번 설정
    }

    void Update() // 매 프레임마다 호출되는 함수
    {
        if (player == null) return; // 플레이어가 없으면 아무것도 하지 않음

        float distance = Vector3.Distance(transform.position, player.position); // 몬스터와 플레이어 사이의 거리 계산

        if (distance <= detectRange) // 플레이어가 감지 범위 내에 있을 때
        {
            MoveTowards(player.position); // 플레이어를 향해 이동

            if (distance <= attackRange && Time.time - lastAttackTime > attackCooldown) // 공격 범위 내이고 쿨타임이 지났을 때
            {
                AttackPlayer(); // 플레이어 공격
                lastAttackTime = Time.time; // 마지막 공격 시각 갱신
            }
        }
        else // 플레이어가 감지 범위 밖에 있을 때
        {
            Wander(); // 자유롭게 이동
        }
    }

    void MoveTowards(Vector3 target) // 지정한 위치로 이동하는 함수
    {
        Vector3 direction = (target - transform.position).normalized; // 목표 위치까지의 방향 벡터 계산 및 정규화
        transform.position += direction * moveSpeed * Time.deltaTime; // 해당 방향으로 이동
        // 필요시 회전 추가
    }

    void Wander() // 자유 이동(랜덤 이동 등) 함수
    {
        wanderTimer -= Time.deltaTime; // 타이머를 매 프레임마다 감소시킴
        if (wanderTimer <= 0f) // 타이머가 0 이하가 되면
        {
            SetRandomWanderDirection(); // 새로운 랜덤 방향을 설정
        }
        transform.position += wanderDirection * moveSpeed * Time.deltaTime; // 현재 방향으로 이동
    }

    // 랜덤 방향을 설정하는 함수
    void SetRandomWanderDirection() // 새로운 랜덤 방향을 설정하는 함수
    {
        float angle = Random.Range(0f, 360f); // 0~360도 중에서 랜덤 각도를 선택
        wanderDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized; // 평면상 랜덤 방향 벡터 계산
        wanderTimer = wanderChangeInterval; // 타이머를 다시 초기화
    }

    void AttackPlayer() // 플레이어를 공격하는 함수
    {
        // 플레이어에게 데미지 주기
        // 예시: player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

    public void TakeDamage(int damage) // 몬스터가 데미지를 받는 함수
    {
        currentHealth -= damage; // 받은 데미지만큼 체력 감소
        if (currentHealth <= 0) // 체력이 0 이하가 되면
        {
            Die(); // 사망 처리
        }
    }

    void Die() // 몬스터가 죽었을 때 호출되는 함수
    {
        // 경험치 구슬 생성 위치
        // TODO: 경험치 구슬 프리팹 생성 및 드롭
        // 예시: Instantiate(expOrbPrefab, transform.position, Quaternion.identity);

        // 아이템 드롭 위치
        // TODO: 아이템 프리팹 생성 및 드롭
        // 예시: Instantiate(itemPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject); // 몬스터 오브젝트 삭제
    }
}
