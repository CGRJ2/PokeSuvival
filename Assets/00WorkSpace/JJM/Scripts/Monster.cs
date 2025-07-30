using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Monster : MonoBehaviourPun, IDamagable
{
    [Header("기본 스탯")]
    public PokemonData pokemonData; // PokemonData ScriptableObject 참조 (Inspector에서 할당)
    [SerializeField] public int level = 1; // 몬스터 레벨 (Inspector에서 수정 가능)
    [SerializeField] public int maxHealth = 100; // 최대 체력 (Inspector에서 수정 가능)
    [SerializeField] public int currentHealth = 100; // 현재 체력 (Inspector에서 수정 가능)

    [Header("AI 설정")]
    public float detectRange = 10f;      // 플레이어를 감지할 수 있는 거리
    public float moveSpeed = 3f;         // 몬스터의 이동 속도
    public float attackRange = 2f;       // 플레이어를 공격할 수 있는 거리
    public float attackCooldown = 1.5f;  // 공격 후 다시 공격할 때까지의 대기 시간

    private Transform player;            // 플레이어의 Transform 참조
    private float lastAttackTime;        // 마지막 공격 시각

    private Vector3 wanderDirection; // 현재 이동 방향을 저장하는 변수
    private float wanderTimer; // 방향을 바꿀 때까지 남은 시간을 저장하는 변수
    public float wanderChangeInterval = 2f; // 방향을 바꿀 시간 간격(초)

    [SerializeField] private Sprite idleSprite;           // 대기(멈춤) 상태 스프라이트
    [SerializeField] private Sprite moveLeftSprite;       // 왼쪽 이동 스프라이트
    [SerializeField] private Sprite moveRightSprite;      // 오른쪽 이동 스프라이트
    [SerializeField] private Sprite moveUpSprite;         // 위쪽 이동 스프라이트
    [SerializeField] private Sprite moveDownSprite;       // 아래쪽 이동 스프라이트
    [SerializeField] private Sprite moveUpLeftSprite;     // 왼쪽 위 대각선 이동 스프라이트
    [SerializeField] private Sprite moveUpRightSprite;    // 오른쪽 위 대각선 이동 스프라이트
    [SerializeField] private Sprite moveDownLeftSprite;   // 왼쪽 아래 대각선 이동 스프라이트
    [SerializeField] private Sprite moveDownRightSprite;  // 오른쪽 아래 대각선 이동 스프라이트
    [SerializeField] private Sprite deadSprite; // 몬스터가 죽었을 때 사용할 스프라이트

    private SpriteRenderer spriteRenderer; // SpriteRenderer 컴포넌트 참조

    [SerializeField] private int attackDamage = 10; // 몬스터 공격력

    private Rigidbody2D rb; // Rigidbody2D 컴포넌트 참조 변수

    [SerializeField] private float corpseDuration = 2f; // 시체가 남아있을 시간(초) (Inspector에서 조정 가능)

    [SerializeField] private GameObject expOrbPrefab; // Inspector에서 경험치 구슬 프리팹 할당

    public BattleDataTable BattleData => throw new System.NotImplementedException();

    void Start() // 게임 시작 시 호출되는 함수
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트 가져오기

        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 컴포넌트 가져오기

        // 회전 고정 추가
        if (rb != null)
            rb.freezeRotation = true;


        if (pokemonData != null) // PokemonData가 할당되어 있으면
        {
            currentHealth = maxHealth; // 현재 체력 초기화
            // 필요하다면 다른 스탯도 pokemonData에서 가져와서 초기화
        }
        else
        {
            currentHealth = maxHealth; // PokemonData가 없으면 Inspector 값 사용
        }
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // "Player" 태그를 가진 오브젝트의 Transform을 찾음
        SetRandomWanderDirection(); // 시작할 때 랜덤 방향을 한 번 설정
    }

    void Update() // 매 프레임마다 호출되는 함수
    {
        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트만 몬스터 AI를 제어

        if (player == null) // 플레이어가 없으면
        {
            Wander(); // 자유롭게 이동
            return; // 아래 로직은 실행하지 않음
        }

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

        Vector2 direction = (target - transform.position).normalized; // 목표 방향 계산
        Vector2 newPos = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime; // 이동할 위치 계산
        rb.MovePosition(newPos); // Rigidbody2D를 이용해 이동(충돌 자동 처리)
        SetSpriteByDirection(direction); // 스프라이트 변경

    }

    void Wander() // 자유 이동(랜덤 이동 등) 함수
    {
        wanderTimer -= Time.deltaTime; // 타이머를 매 프레임마다 감소시킴
        if (wanderTimer <= 0f) // 타이머가 0 이하가 되면
        {
            SetRandomWanderDirection(); // 새로운 랜덤 방향을 설정
        }
        transform.position += wanderDirection * moveSpeed * Time.deltaTime; // 현재 방향으로 이동

        SetSpriteByDirection(wanderDirection); // 방향에 따라 스프라이트 설정
    }

    // 랜덤 방향을 설정하는 함수
    void SetRandomWanderDirection() // 새로운 랜덤 방향을 설정하는 함수
    {
        float angle = Random.Range(0f, 360f); // 0~360도 중에서 랜덤 각도를 선택
        wanderDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized; // 평면상 랜덤 방향 벡터 계산
        wanderTimer = wanderChangeInterval; // 타이머를 다시 초기화
    }

    void AttackPlayer() // 플레이어를 공격하는 함수
    {

        // 플레이어에게 데미지 주기
        if (player == null) return;


        // IDamagable 인터페이스를 가진 컴포넌트 찾기
        IDamagable damagable = player.GetComponent<IDamagable>();
        if (damagable != null)
        {
            // 공격자 정보(BattleDataTable)와 사용할 스킬(PokemonSkill)을 전달해야 함
            // 예시: 기본 공격(스킬이 null이거나 기본값)
            damagable.TakeDamage(this.BattleData, null);

        }
        
    }

    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        // 예시: skill이 null이면 일반 공격, 아니면 스킬 공격 처리
        int damage = 0;
        if (skill != null)
            damage = skill.Damage;
        else
            damage = attackerData.AllStat.Attak; // 또는 적절한 기본 공격력

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
            return true; // 사망
        }
        return false; // 생존
    }

    void Die() // 몬스터가 죽었을 때 호출되는 함수
    {

        // 경험치 구슬 생성
        if (expOrbPrefab != null)
        {
            // 경험치 양은 몬스터 레벨이나 기타 값으로 결정
            int expAmount = level * 10; // 예시: 레벨 x 10
            GameObject orbObj = PhotonNetwork.Instantiate(expOrbPrefab.name, transform.position, Quaternion.identity);
            ExpOrb orb = orbObj.GetComponent<ExpOrb>();
            if (orb != null)
                orb.Init(expAmount);
        }
        

        StartCoroutine(CorpseAndDestroy()); // 시체 유지 후 삭제 코루틴 시작

    }

    IEnumerator CorpseAndDestroy() // 시체를 일정 시간 남겼다가 삭제하는 코루틴
    {
        // 죽은 상태 연출(스프라이트 변경, 반투명 등)
        if (spriteRenderer != null)
        {
            if (deadSprite != null)
                spriteRenderer.sprite = deadSprite; // 죽음 스프라이트로 변경
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); // 반투명 처리(예시)
        }
        // 콜라이더/AI 비활성화 (움직임, 충돌 방지)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false; // 콜라이더 비활성화
        this.enabled = false; // 몬스터 스크립트 비활성화(추가 동작 방지)

        yield return new WaitForSeconds(corpseDuration); // 지정한 시간만큼 대기

        PhotonNetwork.Destroy(gameObject); // 네트워크에서 몬스터 오브젝트 삭제
    }

    void SetSpriteByDirection(Vector2 dir) // 방향에 따라 스프라이트를 설정하는 함수
    {
        if (spriteRenderer == null) return; // SpriteRenderer가 없으면 리턴

        // 방향이 거의 0이면 idle 처리
        if (dir.magnitude < 0.01f)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        // 대각선 우선 판별 (45도 단위)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // 방향 각도 계산

        if (angle >= -22.5f && angle < 22.5f)
            spriteRenderer.sprite = moveRightSprite; // 오른쪽
        else if (angle >= 22.5f && angle < 67.5f)
            spriteRenderer.sprite = moveUpRightSprite; // 오른쪽 위
        else if (angle >= 67.5f && angle < 112.5f)
            spriteRenderer.sprite = moveUpSprite; // 위
        else if (angle >= 112.5f && angle < 157.5f)
            spriteRenderer.sprite = moveUpLeftSprite; // 왼쪽 위
        else if (angle >= 157.5f || angle < -157.5f)
            spriteRenderer.sprite = moveLeftSprite; // 왼쪽
        else if (angle >= -157.5f && angle < -112.5f)
            spriteRenderer.sprite = moveDownLeftSprite; // 왼쪽 아래
        else if (angle >= -112.5f && angle < -67.5f)
            spriteRenderer.sprite = moveDownSprite; // 아래
        else if (angle >= -67.5f && angle < -22.5f)
            spriteRenderer.sprite = moveDownRightSprite; // 오른쪽 아래
    }


    public void ResetMonster() // 몬스터 상태 초기화 함수
    {
        currentHealth = maxHealth; // 체력 초기화
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white; // 색상(투명도) 초기화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true; // 콜라이더 활성화
        this.enabled = true; // 스크립트 활성화
        // 필요하다면 추가로 상태/애니메이션/AI 등도 초기화
    }

    // Photon 네트워크를 통한 동기화 함수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // 네트워크 동기화 함수
    {
        if (stream.IsWriting) // 마스터 클라이언트에서 데이터를 보낼 때
        {
            stream.SendNext(transform.position); // 위치 정보 전송
            stream.SendNext(currentHealth); // 체력 정보 전송
            stream.SendNext(level); // 레벨 정보 전송
        }
        else // 다른 클라이언트에서 데이터를 받을 때
        {
            transform.position = (Vector3)stream.ReceiveNext(); // 위치 정보 수신
            currentHealth = (int)stream.ReceiveNext(); // 체력 정보 수신
            level = (int)stream.ReceiveNext(); // 레벨 정보 수신
        }
    }
}
