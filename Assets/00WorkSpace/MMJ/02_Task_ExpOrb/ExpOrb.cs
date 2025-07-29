using System;
using Photon.Pun;
using UnityEngine;

public class ExpOrb : MonoBehaviourPun
{
    public int amount;
    public bool isActive;

    [Header("자석효과 설정")]
    public float magnetRadius = 3f;
    public float moveSpeed = 5f;
    public float accelerationRate = 0.5f;

    public bool isAttracted = false;
    public Transform playerTransform;
    public float currentSpeed;

    public event Action<ExpOrb> OnDespawned;


    public void Init(int amount)
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
            CheckForNearbyPlayers();
        }
    }

    private void CheckForNearbyPlayers()
    {
        // null 체크 추가
        if (photonView == null)
        {
            Debug.LogWarning("photonView가 null입니다. 컴포넌트를 확인해주세요.");
            return;
        }

        // 레이어 마스크 생성
        int playerLayerMask = LayerMask.GetMask("Player");

        // 콜라이더 검색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, magnetRadius, playerLayerMask);

        if (colliders.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Transform closestPlayer = null;

            foreach (Collider2D col in colliders)
            {
                if (col == null || col.transform == null) continue;

                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = col.transform;
                }
            }

            if (closestPlayer != null)
            {
                playerTransform = closestPlayer;
                isAttracted = true;

                // PhotonView 접근
                PhotonView playerView = closestPlayer.GetComponent<PhotonView>();
                if (PhotonNetwork.IsConnected && photonView.IsMine && playerView != null)
                {
                    photonView.RPC(nameof(RPC_StartAttraction), RpcTarget.Others, playerView.ViewID);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_StartAttraction(int playerViewID)
    {
        // 플레이어 ViewID로 해당 플레이어 찾기
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView != null)
        {
            playerTransform = playerView.transform;
            isAttracted = true;
        }
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 플레이어에 닿으면 먹힘
        if (collider.CompareTag("Player"))
        {
            // 경험치 전달하는 로직 추가 가능
            Debug.Log($"플레이어 경험치 획득 : {amount}");
            var pc = collider.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.AddExp(amount);
            }
            photonView.RPC(nameof(RPC_Despawn), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_Despawn()
    {
        isActive = false;
        isAttracted = false;
        if (PhotonNetwork.IsMasterClient) OnDespawned?.Invoke(this);
        gameObject.SetActive(false);
    }
    // 디버깅을 위해 자석 범위를 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
