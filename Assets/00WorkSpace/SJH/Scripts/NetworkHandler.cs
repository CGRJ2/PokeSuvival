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
	public void RPC_SetStatus(string skillName)
	{
		if (PC.Status == null) PC.SetStatus(new PokeStatusHandler(this, PC.Model));

		// 상태이상 UI를 업데이트할 클라이언트
		if (PC.photonView.IsMine)
		{
			PC.Status.SetStatus(skillName, true);
			// TODO : 상태이상에 따라 디버프 적용
			var skill = Define.GetPokeSkillData(skillName);
			if (skill == null) return;
			switch (skill.StatusEffect)
			{
				case StatusType.Burn: Debug.Log("화상 걸림"); PC.Status.SetBurnDamage(skill.StatusDuration); break;
				case StatusType.Poison: Debug.Log("독 걸림"); PC.Status.SetPoisonDamage(skill.StatusDuration); break;
				case StatusType.Freeze: Debug.Log("동상 걸림"); PC.Status.SetFreeze(skill.StatusDuration); break;
				case StatusType.Binding: Debug.Log("속박 걸림"); PC.Status.SetBinding(skill.StatusDuration); break;
				case StatusType.Paralysis: Debug.Log("마비 걸림"); PC.Status.SetParalysis(skill.StatusDuration); break;
				case StatusType.Confusion: Debug.Log("혼란 걸림"); PC.Status.SetConfusion(skill.StatusDuration); break;
			}
		}
		else
		{
			PC.Status.SetStatus(skillName, false);
		}
	}
	[PunRPC]
	public void RPC_RemoveStatus(string skillName)
	{

	}
	[PunRPC]
	public void RPC_SetHit()
	{
		PC.View.SetIsHit();
	}
	[PunRPC]
	public void RPC_BuffSync(int viewId, string skillName)
	{
		var pv = PhotonView.Find(viewId);
		if (pv == null)
		{
			Debug.LogWarning("RPC_BuffSync : photonView == null");
			return;
		}
		var pc = pv.GetComponent<PlayerController>();
		if (pc == null)
		{
			Debug.LogWarning("RPC_BuffSync : PlayerController == null");
			return;
		}
		if (pc.Buff == null)
		{
			Debug.LogWarning("RPC_BuffSync : Buff == null");
			return;
		}
		if (pc.Model == null)
		{
			Debug.LogWarning("RPC_BuffSync : Model == null");
			return;
		}
		pc.SetBuff(new PokeBuffHandler(pc, pc.Model));
		Debug.Log($"{viewId} : [{skillName} 버프] 동기화 시작");
		pc.Buff.SetBuff(skillName);
	}
}
