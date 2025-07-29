using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Monster : MonoBehaviourPun, IPunObservable
{
    [Header("�⺻ ����")]
    public PokemonData pokemonData; // PokemonData ScriptableObject ���� (Inspector���� �Ҵ�)
    [SerializeField] public int level = 1; // ���� ���� (Inspector���� ���� ����)
    [SerializeField] public int maxHealth = 100; // �ִ� ü�� (Inspector���� ���� ����)
    [SerializeField] public int currentHealth = 100; // ���� ü�� (Inspector���� ���� ����)

    [Header("AI ����")]
    public float detectRange = 10f;      // �÷��̾ ������ �� �ִ� �Ÿ�
    public float moveSpeed = 3f;         // ������ �̵� �ӵ�
    public float attackRange = 2f;       // �÷��̾ ������ �� �ִ� �Ÿ�
    public float attackCooldown = 1.5f;  // ���� �� �ٽ� ������ �������� ��� �ð�
    
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

    private SpriteRenderer spriteRenderer; // SpriteRenderer ������Ʈ ����

    [SerializeField] private int attackDamage = 10; // ���� ���ݷ�
    void Start() // ���� ���� �� ȣ��Ǵ� �Լ�
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer ������Ʈ ��������


        if (pokemonData != null) // PokemonData�� �Ҵ�Ǿ� ������
        {
            currentHealth = maxHealth; // ���� ü�� �ʱ�ȭ
            // �ʿ��ϴٸ� �ٸ� ���ȵ� pokemonData���� �����ͼ� �ʱ�ȭ
        }
        else
        {
            currentHealth = maxHealth; // PokemonData�� ������ Inspector �� ���
        }
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // "Player" �±׸� ���� ������Ʈ�� Transform�� ã��
        SetRandomWanderDirection(); // ������ �� ���� ������ �� �� ����
    }

    void Update() // �� �����Ӹ��� ȣ��Ǵ� �Լ�
    {
        if (!PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ�� ���� AI�� ����

        if (player == null) // �÷��̾ ������
        {
            Wander(); // �����Ӱ� �̵�
            return; // �Ʒ� ������ �������� ����
        }

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
        // ������ �� idle ��������Ʈ�� ����
        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;

    }

    void MoveTowards(Vector3 target) // ������ ��ġ�� �̵��ϴ� �Լ�
    {
        Vector3 direction = (target - transform.position).normalized; // ��ǥ ��ġ������ ���� ���� ��� �� ����ȭ
        transform.position += direction * moveSpeed * Time.deltaTime; // �ش� �������� �̵�
                                                                      // �ʿ�� ȸ�� �߰�

        SetSpriteByDirection(direction); // ���⿡ ���� ��������Ʈ ����

    }

    void Wander() // ���� �̵�(���� �̵� ��) �Լ�
    {
        wanderTimer -= Time.deltaTime; // Ÿ�̸Ӹ� �� �����Ӹ��� ���ҽ�Ŵ
        if (wanderTimer <= 0f) // Ÿ�̸Ӱ� 0 ���ϰ� �Ǹ�
        {
            SetRandomWanderDirection(); // ���ο� ���� ������ ����
        }
        transform.position += wanderDirection * moveSpeed * Time.deltaTime; // ���� �������� �̵�

        SetSpriteByDirection(wanderDirection); // ���⿡ ���� ��������Ʈ ����
    }

    // ���� ������ �����ϴ� �Լ�
    void SetRandomWanderDirection() // ���ο� ���� ������ �����ϴ� �Լ�
    {
        float angle = Random.Range(0f, 360f); // 0~360�� �߿��� ���� ������ ����
        wanderDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized; // ���� ���� ���� ���� ���
        wanderTimer = wanderChangeInterval; // Ÿ�̸Ӹ� �ٽ� �ʱ�ȭ
    }

    void AttackPlayer() // �÷��̾ �����ϴ� �Լ�
    {

        // �÷��̾�� ������ �ֱ�
        if (player == null) return;

        // IDamageable �������̽��� ���� ������Ʈ ã��
        IDamagable damagable = player.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(attackDamage);
        }
        
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

        PhotonNetwork.Destroy(gameObject); // ��Ʈ��ũ���� ���� ������Ʈ ����
    }

    void SetSpriteByDirection(Vector2 dir) // ���⿡ ���� ��������Ʈ�� �����ϴ� �Լ�
    {
        if (spriteRenderer == null) return; // SpriteRenderer�� ������ ����

        // ������ ���� 0�̸� idle ó��
        if (dir.magnitude < 0.01f)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        // �밢�� �켱 �Ǻ� (45�� ����)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // ���� ���� ���

        if (angle >= -22.5f && angle < 22.5f)
            spriteRenderer.sprite = moveRightSprite; // ������
        else if (angle >= 22.5f && angle < 67.5f)
            spriteRenderer.sprite = moveUpRightSprite; // ������ ��
        else if (angle >= 67.5f && angle < 112.5f)
            spriteRenderer.sprite = moveUpSprite; // ��
        else if (angle >= 112.5f && angle < 157.5f)
            spriteRenderer.sprite = moveUpLeftSprite; // ���� ��
        else if (angle >= 157.5f || angle < -157.5f)
            spriteRenderer.sprite = moveLeftSprite; // ����
        else if (angle >= -157.5f && angle < -112.5f)
            spriteRenderer.sprite = moveDownLeftSprite; // ���� �Ʒ�
        else if (angle >= -112.5f && angle < -67.5f)
            spriteRenderer.sprite = moveDownSprite; // �Ʒ�
        else if (angle >= -67.5f && angle < -22.5f)
            spriteRenderer.sprite = moveDownRightSprite; // ������ �Ʒ�
    }

    // Photon ��Ʈ��ũ�� ���� ����ȭ �Լ�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // ��Ʈ��ũ ����ȭ �Լ�
    {
        if (stream.IsWriting) // ������ Ŭ���̾�Ʈ���� �����͸� ���� ��
        {
            stream.SendNext(transform.position); // ��ġ ���� ����
            stream.SendNext(currentHealth); // ü�� ���� ����
            stream.SendNext(level); // ���� ���� ����
        }
        else // �ٸ� Ŭ���̾�Ʈ���� �����͸� ���� ��
        {
            transform.position = (Vector3)stream.ReceiveNext(); // ��ġ ���� ����
            currentHealth = (int)stream.ReceiveNext(); // ü�� ���� ����
            level = (int)stream.ReceiveNext(); // ���� ���� ����
        }
    }
}
