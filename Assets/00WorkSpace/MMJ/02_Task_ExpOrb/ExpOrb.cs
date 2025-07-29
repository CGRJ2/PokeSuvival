using UnityEngine;
using System;
using Photon.Pun;

public class ExpOrb : MonoBehaviourPun
{
    public int amount;
    public bool isActive;

    public event Action<ExpOrb> OnDespawned;

    public void Init(int amount)
    {
        this.amount = amount;
        isActive = true;
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
        if (PhotonNetwork.IsMasterClient) OnDespawned?.Invoke(this);
		gameObject.SetActive(false);
	}
}