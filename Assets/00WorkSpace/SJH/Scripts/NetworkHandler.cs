using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkHandler: MonoBehaviour
{
	public PlayerController PC { get; private set; }
	public PhotonView PV { get; private set; }

	void Awake()
	{
		PC = GetComponent<PlayerController>();
		PV = PC.photonView;
	}

	public void ActionRPC(string funcName, RpcTarget target, params object[] value) => PV.RPC(funcName, target, value);
	public void ActionRPC(string funcName, Player targetPlayer, params object[] value) => PV.RPC(funcName, targetPlayer, value);

	[PunRPC]
	public void RPC_PokemonEvolution(int pokeNumber, PhotonMessageInfo info)
	{
		PokemonData pokeData = Define.GetPokeData(pokeNumber);
		PC.Model.PokemonEvolution(pokeData);
		PC.View.SetAnimator(pokeData.AnimController);
		// TODO : 진화 연출
	}
	[PunRPC]
	public void RPC_ChangePokemonData(string nickName, string userId, int pokeNumber)
	{
		var pokeData = Define.GetPokeData(pokeNumber);
		PC.SetModel(new PlayerModel(nickName, userId, pokeData));
		PC.View?.SetAnimator(pokeData.AnimController);
		PC.View?.SetColliderSize(pokeData.PokeSize);

		if (PhotonNetwork.LocalPlayer.IsLocal) PC.OnModelChanged?.Invoke(PC.Model);
	}
	[PunRPC]
	public void RPC_CurrentHpChanged(int value)
	{
		if (!PV.IsMine) PC.Model.SetCurrentHp(value);
	}
	[PunRPC]
	public void RPC_LevelChanged(int value, PhotonMessageInfo info)
	{
		if (!PV.IsMine)
		{
			PC.Model.SetLevel(value);
		}
		else
		{
			var currentData = PC.Model.PokeData;
			var nextData = PC.Model.PokeData.NextEvoData;
			if (nextData != null && value >= currentData.EvoLevel)
			{
				Debug.Log($"{value} >= {currentData.EvoLevel}");
				ActionRPC(nameof(RPC_PokemonEvolution), RpcTarget.All, nextData.PokeNumber);
			}
		}
	}
	[PunRPC]
	public void RPC_TakeDamage(int value)
	{
		if (value > 0) PC.View.SetIsHit();
		Debug.Log($"{value} 대미지 입음");
		if (PV.IsMine)
		{
			PC.Model.SetCurrentHp(PC.Model.CurrentHp - value);
		}
		PlayerManager.Instance.ShowDamageText(PC.transform, value, Color.red);
	}
	[PunRPC]
	public void RPC_SyncToNewPlayer(string nickName, string userId, int pokeNumber, int level, int currentHp)
	{
		// 새로 접속한 클라이언트에서 기존 플레이어 오브젝트 데이터 초기화
		var pokeData = Define.GetPokeData(pokeNumber);
		PC.SetModel(new PlayerModel(nickName, userId, pokeData, level, 0, currentHp));
		//PC.SetRank(new PokeRankHandler(PC, PC.Model));
		PC.View?.SetAnimator(pokeData.AnimController);
		PC.View?.SetColliderSize(pokeData.PokeSize);

		if (PhotonNetwork.LocalPlayer.IsLocal) PC.OnModelChanged?.Invoke(PC.Model);
	}
	[PunRPC]
	public void RPC_PlayerSetActive(bool value)
	{
		PC.gameObject.SetActive(value);
	}
	[PunRPC]
	public void RPC_SetLastAttacker(int viewId)
	{
		if (!PV.IsMine) return;
		if (viewId < 0) PC.SetLastAttacker(null);

		var pv = PhotonView.Find(viewId);
		if (pv != null) PC.SetLastAttacker(pv.GetComponent<PlayerController>());
	}
	[PunRPC]
	public void RPC_AddKillCount()
	{
		PC.AddKillCount();
		Debug.Log($"킬 증가! 현재 킬 : {PC.KillCount}");
	}
	[PunRPC]
	public void RPC_RankSync(int viewId, int statTypeIndex, int value)
	{
		var pv = PhotonView.Find(viewId);
		if (pv == null)
		{
			Debug.LogWarning("RPC_RankSync : photonView == null");
			return;
		}
		var pc = pv.GetComponent<PlayerController>();
		if (pc == null)
		{
			Debug.LogWarning("RPC_RankSync : PlayerController == null");
			return;
		}
		if (pc.Rank == null)
		{
			Debug.LogWarning("RPC_RankSync : Rank == null");
			return;
		}
		if (pc.Model == null)
		{
			Debug.LogWarning("RPC_RankSync : Model == null");
			return;
		}
		pc.SetRank(new PokeRankHandler(pc, pc.Model));
		StatType statType = (StatType)statTypeIndex;
		Debug.Log($"{viewId} : [{statType} : {value}] 동기화 시작");
		pc.Rank.RankSync(statType, value);
	}
	[PunRPC]
	public void RPC_TotalExpChanged(int value)
	{
		if (!PV.IsMine) PC.Model.SetTotalExp(value);
	}
	[PunRPC]
	public void RPC_PlayerDead(int deadExp)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.Log($"플레이어 사망 경험치 구슬 [{deadExp}] 생성");
			PhotonNetwork.InstantiateRoomObject("ExpOrb", transform.position, Quaternion.identity, 0, new object[] { deadExp });
		}
	}
	[PunRPC]
	public void RPC_SetStatus(string skillName, int statusIndex, float duration)
	{
		// TODO : 스킬이름으로 스킬 SO 받아오기
		if (PC.Status == null) PC.SetStatus(new PokeStatusHandler(this, PC.Model));
		PC.Status.SetStatus(skillName, (StatusType)statusIndex, duration);
	}
}
