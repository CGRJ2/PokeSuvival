using System;
using Photon.Pun;
using UnityEngine;

public class ExpOrb : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public int amount;
    public bool isActive;

    [Header("자석효과 설정")]
    public float magnetRadius = 3.5f;
    public float moveSpeed = 3f;
    public float accelerationRate = 33f;

    public bool isAttracted = false;
    public Transform playerTransform;
    public float currentSpeed;

    public ExpOrbSpawner spawner;
    //public event Action<ExpOrb> OnDespawned;

    private Collider2D[] overlapResults = new Collider2D[20]; // 최대 인원이 20명이니까 20개까지 감지

    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _searchDelay = 0.1f;
    [SerializeField] private float _nextSearchTime;
    [SerializeField] private bool _isEntityOrb = false;

    public void Init(int amount)
    {
        this.amount = amount;
        isActive = true;
        photonView.RPC(nameof(RPC_Init), RpcTarget.OthersBuffered, amount);
    }

    [PunRPC]
    public void RPC_Init(int amount)
    {
        this.amount = amount;
        isActive = true;
        isAttracted = false;
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!isActive || !PhotonNetwork.IsConnected) return;

        if (isAttracted && playerTransform != null)
        {
            currentSpeed += accelerationRate * Time.deltaTime;

            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
        }
        else
        {
            if (Time.time >= _nextSearchTime)
            {
                _nextSearchTime = Time.time + _searchDelay;
                CheckForNearbyPlayers();
            }
        }
    }

    private void CheckForNearbyPlayers()
    {
        if (photonView == null)
        {
            Debug.LogWarning("photonView가 null입니다. 컴포넌트를 확인해주세요.");
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, magnetRadius, overlapResults, _playerLayer);

        if (hitCount > 0)
        {
            float closestDistance = float.MaxValue;
            Transform closestPlayer = null;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = overlapResults[i];
                if (col == null || col.transform == null) continue;

                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = col.transform;
                }
            }

            if (closestPlayer != null && closestPlayer.TryGetComponent<PlayerController>(out var pc) && pc.Model.CurrentHp > 0)
            {
                playerTransform = closestPlayer;
				isAttracted = true;
			}
        }

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 플레이어에 닿으면 먹힘
        if (collider.CompareTag("Player"))
        {
            // 경험치 전달하는 로직 추가 가능
            var pc = collider.GetComponent<PlayerController>();
            if (pc != null && pc.Model.CurrentHp > 0)
            {
                Debug.Log($"플레이어 경험치 획득 : {amount}");
                pc.AddExp(amount);
                photonView.RPC(nameof(RPC_Despawn), RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void RPC_Despawn()
    {
        isActive = false;
        isAttracted = false;
        if (PhotonNetwork.IsMasterClient)
        {
            if (_isEntityOrb) PhotonNetwork.Destroy(gameObject);
            else spawner.HandleOrbDespawned(this);
        }

        gameObject.SetActive(false);
    }
    // 디버깅을 위해 자석 범위를 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
        var data = photonView.InstantiationData;
        if (data == null) return;
        if (data?.Length == 0) return;
        int exp = (int)data[0];
        amount = exp;
        isActive = true;
        _isEntityOrb = true;
	}
}
