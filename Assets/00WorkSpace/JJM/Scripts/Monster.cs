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

    private Transform player;            // �÷��̾��� Transform ����
    private float lastAttackTime;        // ������ ���� �ð�

    private Vector3 wanderDirection; // ���� �̵� ������ �����ϴ� ����
    private float wanderTimer; // ������ �ٲ� ������ ���� �ð��� �����ϴ� ����
    public float wanderChangeInterval = 2f; // ������ �ٲ� �ð� ����(��)

    [SerializeField] private Sprite idleSprite;           // ���(����) ���� ��������Ʈ
    [SerializeField] private Sprite moveLeftSprite;       // ���� �̵� ��������Ʈ
    [SerializeField] private Sprite moveRightSprite;      // ������ �̵� ��������Ʈ
    [SerializeField] private Sprite moveUpSprite;         // ���� �̵� ��������Ʈ
    [SerializeField] private Sprite moveDownSprite;       // �Ʒ��� �̵� ��������Ʈ
    [SerializeField] private Sprite moveUpLeftSprite;     // ���� �� �밢�� �̵� ��������Ʈ
    [SerializeField] private Sprite moveUpRightSprite;    // ������ �� �밢�� �̵� ��������Ʈ
    [SerializeField] private Sprite moveDownLeftSprite;   // ���� �Ʒ� �밢�� �̵� ��������Ʈ
    [SerializeField] private Sprite moveDownRightSprite;  // ������ �Ʒ� �밢��
    [SerializeField] private Sprite deadSprite; // ���Ͱ� �׾��� �� ����� ��������Ʈ

    private SpriteRenderer spriteRenderer; // SpriteRenderer ������Ʈ ����

    [SerializeField] private int attackDamage = 10; // ���� ���ݷ�

    private Rigidbody2D rb; // Rigidbody2D ������Ʈ ���� ����

    [SerializeField] private float corpseDuration = 2f; // ��ü�� �������� �ð�(��) (Inspector���� ���� ����)

    [SerializeField] private GameObject expOrbPrefab; // Inspector���� ����ġ ���� ������ �Ҵ�

    public BattleDataTable BattleData => throw new System.NotImplementedException();

    void Start() // ���� ���� �� ȣ��Ǵ� �Լ�
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ������Ʈ ��������

        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer ������Ʈ ��������



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

        Vector2 direction = (target - transform.position).normalized; // ��ǥ ���� ���
        Vector2 newPos = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime; // �̵��� ��ġ ���
        rb.MovePosition(newPos); // Rigidbody2D�� �̿��� �̵�(�浹 �ڵ� ó��)
        SetSpriteByDirection(direction); // ��������Ʈ ����

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


        // IDamageable �������̽��� ���� ������Ʈ ã��
        IDamagable damagable = player.GetComponent<IDamagable>();
        if (damagable != null)
        {
            //MMJ 버그떠서 임시 주석처리 충돌시 나중에 삭제 요망
            //damagable.TakeDamage(attackDamage);

        }
        
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

        // ����ġ ���� ����
        if (expOrbPrefab != null)
        {
            // ����ġ ���� ���� �����̳� ��Ÿ ������ ����
            int expAmount = level * 10; // ����: ���� x 10
            GameObject orbObj = PhotonNetwork.Instantiate(expOrbPrefab.name, transform.position, Quaternion.identity);
            ExpOrb orb = orbObj.GetComponent<ExpOrb>();
            if (orb != null)
                orb.Init(expAmount);
        }
        

        StartCoroutine(CorpseAndDestroy()); // ��ü ���� �� ���� �ڷ�ƾ ����

    }

    IEnumerator CorpseAndDestroy() // ��ü�� ���� �ð� ����ٰ� �����ϴ� �ڷ�ƾ
    {
        // ���� ���� ����(��������Ʈ ����, ������ ��)
        if (spriteRenderer != null)
        {
            if (deadSprite != null)
                spriteRenderer.sprite = deadSprite; // ���� ��������Ʈ�� ����
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); // ������ ó��(����)
        }
        // �ݶ��̴�/AI ��Ȱ��ȭ (������, �浹 ����)
        Collider2D col = GetComponent<Collider2D>(); // Collider2D ������Ʈ ��������
        if (col != null) col.enabled = false; // �ݶ��̴� ��Ȱ��ȭ
        this.enabled = false; // ���� ��ũ��Ʈ ��Ȱ��ȭ(�߰� ���� ����)

        yield return new WaitForSeconds(corpseDuration); // ������ �ð���ŭ ���


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


    public void ResetMonster() // ���� ���� �ʱ�ȭ �Լ�
    {
        currentHealth = maxHealth; // ü�� �ʱ�ȭ
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white; // ����(������) �ʱ�ȭ
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true; // �ݶ��̴� Ȱ��ȭ
        this.enabled = true; // ��ũ��Ʈ Ȱ��ȭ
                             // �ʿ��ϴٸ� �߰��� ����/�ִϸ��̼�/AI � �ʱ�ȭ
    }

    // Photon ��Ʈ��ũ�� ���� ����ȭ �Լ�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // ��Ʈ��ũ ����ȭ �Լ�

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

    public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
    {
        throw new System.NotImplementedException();
    }
}
