using NTJ;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback, IDamagable, IStatReceiver
{
	[field: SerializeField] public PlayerModel Model { get; private set; }
	[field: SerializeField] public PlayerView View { get; private set; }
	[field: SerializeField] public PokeRankHandler Rank { get; private set; }

	[SerializeField] private PlayerInput _input;
	[SerializeField] private bool _flipX;
	[field: SerializeField] public Vector2 MoveDir { get; private set; }

	private PokemonStat _prevRankedStat;
	[SerializeField] private BattleDataTable _battleData;
	public BattleDataTable BattleData
	{
		get
		{
			if (Rank == null || Rank.BaseData == null)
			{
				Rank = new PokeRankHandler(this, Model);
				ConnectRankEvent();
			}
			var newStat = Rank.GetRankedStat();
			if (!_prevRankedStat.IsEqual(newStat))
			{
				_prevRankedStat = newStat;
				_battleData = new BattleDataTable(Model.PokeLevel, Model.PokeData, _prevRankedStat, Model.MaxHp, Model.CurrentHp, false, this);
			}
			return _battleData;
		}
	}
	public int Test_Level; // 변화할 레벨
	private int _maxLogCount = 10;
	[SerializeField] private Queue<Vector2> _moveHistory = new();
	[SerializeField] private Vector2 _lastDir = Vector2.down;
	public Action<PlayerModel> OnModelChanged;

	// 생존시간
	private float _startTime;
	private float _endTime;
	public float SurvivalTime
	{
		get
		{
			// 아직 살아있으면
			if (_endTime <= 0) return Time.time - _startTime;
			else return _endTime - _startTime;
		}
	}

	// 킬 카운트
	public int KillCount { get; private set; }
	[SerializeField] private PlayerController _lastAttacker;

	// 버프 코루틴
	private Dictionary<StatType, Coroutine> _rankUpRoutineDic = new();

	void Awake()
	{
		View = GetComponent<PlayerView>();
		_input = GetComponent<PlayerInput>();

		_moveHistory = new(_maxLogCount);
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Model.SetLevel(Test_Level);
		}
	}

	public void PlayerInit()
	{
		Debug.Log("플레이어 초기화");

		// TODO : 플레이어 생성 연출

		// 시작, 사망 시간 초기화
		_startTime = -1;
		_endTime = -1;

		// 시작 시간 기록
		_startTime = Time.time;
		// 킬 초기화
		KillCount = 0;
		// 마지막 공격자 초기화
		_lastAttacker = null;

		PokemonData pokeData = null;
		object[] data = photonView.InstantiationData;
		if (data[0] is int pokeNumber) pokeData = Define.GetPokeData(pokeNumber);
		else if (data[0] is string pokeName)
		{
			pokeData = Define.GetPokeData(pokeName);
		}
		ActionRPC(nameof(RPC_ChangePokemonData), RpcTarget.All, pokeData.PokeNumber);
		PlayerManager.Instance.PlayerFollowCam.Follow = transform;
		ConnectEvent();

		PlayerManager.Instance.LocalPlayerController = this;

		OnModelChanged?.Invoke(Model);

		// TODO : 테스트 코드
		GameObject.Find("Button1")?.GetComponent<Button>().onClick.AddListener(() => { StartPokeEvolution(); });
	}

	public void PlayerRespawn()
	{
		// TODO : 플레이어 생성 연출

		// 시작, 사망 시간 초기화
		_startTime = -1;
		_endTime = -1;

		// 시작 시간 기록
		_startTime = Time.time;
		// 킬 초기화
		KillCount = 0;
		// 마지막 공격자 초기화
		_lastAttacker = null;

		_input.actions.Enable();
		View.SetIsDead(false);
		// 플레이어의 커스텀프로퍼티로 사용할 포켓몬 지정
		string pokemonName = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
		PokemonData pokeData = Define.GetPokeData(pokemonName);
		if (pokeData == null)
		{
			object[] data = photonView.InstantiationData;
			if (data[0] is int pokeNumber) pokeData = Define.GetPokeData(pokeNumber);
			else if (data[0] is string pokeName)
			{
				pokeData = Define.GetPokeData(pokeName);
			}
		}
		ActionRPC(nameof(RPC_ChangePokemonData), RpcTarget.All, pokeData.PokeNumber);
		ActionRPC(nameof(RPC_PlayerSetActive), RpcTarget.AllBuffered, true);
		PlayerManager.Instance.PlayerFollowCam.Follow = transform;
		ConnectEvent();

		PlayerManager.Instance.LocalPlayerController = this;
	}

	public void ConnectEvent()
	{
		DisconnectEvent();

		Model.OnCurrentHpChanged += (hp) =>
		{
			ActionRPC(nameof(RPC_CurrentHpChanged), RpcTarget.All, hp);
		};
		Model.OnDied += () =>
		{
			// 사망 시간 기록
			_endTime = Time.time;
			// 막타친 플레이어 킬카운트 증가
			if (_lastAttacker != null) ActionRPC(nameof(RPC_AddKillCount), _lastAttacker.photonView.Owner);

			_input.actions.Disable();
			View.SetIsDead(true);
			Debug.LogWarning("플레이어 사망");
			PlayerManager.Instance.PlayerDead(Model.TotalExp);
		};
		Model.OnPokeLevelChanged += (level) => { ActionRPC(nameof(RPC_LevelChanged), RpcTarget.All, level); };
		OnModelChanged += (model) =>
		{
			if (model != null) Rank = new PokeRankHandler(this, model);
			ConnectRankEvent();
			UIManager.Instance.InGameGroup.UpdateSkillSlots(model);
		};

		ConnectSkillEvent();
		ConnectRankEvent();
	}

	public void DisconnectEvent()
	{
		Model.OnCurrentHpChanged = null;
		Model.OnDied = null;
		Model.OnPokeLevelChanged = null;
		OnModelChanged = null;

		DisconnectSkillEvent();
		DisconnectRankEvent();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
		if (stream.IsWriting)
		{
			stream.SendNext(_flipX);
			// TODO : 실시간 동기화
		}
		else
		{
			View.SetFlip(_flipX = (bool)stream.ReceiveNext());
			// TODO : 실시간 동기화
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!photonView.IsMine)
		{
			gameObject.tag = "Enemy";
			View.SetOrderInLayer(false);
			return;
		}
		gameObject.tag = "Player";
		View.SetOrderInLayer(true);
		PlayerInit();
	}

	public void ConnectSkillEvent()
	{
		var input = _input.actions;
		input["Skill1"].started += OnSkillSlot1;
		input["Skill1"].canceled += OnSkillSlot1;
		input["Skill2"].started += OnSkillSlot2;
		input["Skill2"].canceled += OnSkillSlot2;
		input["Skill3"].started += OnSkillSlot3;
		input["Skill3"].canceled += OnSkillSlot3;
		input["Skill4"].started += OnSkillSlot4;
		input["Skill4"].canceled += OnSkillSlot4;
	}
	public void DisconnectSkillEvent()
	{
		var input = _input.actions;
		input["Skill1"].started -= OnSkillSlot1;
		input["Skill1"].canceled -= OnSkillSlot1;
		input["Skill2"].started -= OnSkillSlot2;
		input["Skill2"].canceled -= OnSkillSlot2;
		input["Skill3"].started -= OnSkillSlot3;
		input["Skill3"].canceled -= OnSkillSlot3;
		input["Skill4"].started -= OnSkillSlot4;
		input["Skill4"].canceled -= OnSkillSlot4;
	}
	private void OnSkillSlot1(InputAction.CallbackContext ctx) => OnSkill(SkillSlot.Skill1, ctx);
	private void OnSkillSlot2(InputAction.CallbackContext ctx) => OnSkill(SkillSlot.Skill2, ctx);
	private void OnSkillSlot3(InputAction.CallbackContext ctx) => OnSkill(SkillSlot.Skill3, ctx);
	private void OnSkillSlot4(InputAction.CallbackContext ctx) => OnSkill(SkillSlot.Skill4, ctx);

	public void ConnectRankEvent()
	{
		if (Rank == null) return;

		Rank.OnRankChanged += (statType, prev, next) =>
		{
			// 스탯종류, 이전값, 이후값
		};

		Rank.OnSyncToRank += (statType, next) =>
		{
			if (!photonView.IsMine) return;
			// 스탯종류, 최신값
			Debug.Log($"{Model.PokeData.PokeName} [{statType} : {next}] 동기화 시작");
			ActionRPC(nameof(RPC_RankSync), RpcTarget.OthersBuffered, photonView.ViewID, (int)statType, next);
		};
	}

	public void DisconnectRankEvent()
	{
		if (Rank == null) return;

		Rank.OnRankChanged = null;
		Rank.OnSyncToRank = null;
	}

	void MoveInput()
	{
		if (MoveDir.x != 0) _flipX = MoveDir.x > 0.1f;

		if (MoveDir != Vector2.zero)
		{
			_moveHistory.Enqueue(MoveDir);
			if (_moveHistory.Count > _maxLogCount) _moveHistory.Dequeue();
		}

		//_view.PlayerMove(MoveDir, _model.GetMoveSpeed());
		View.PlayerMove(MoveDir, _lastDir, Model.GetMoveSpeed());
	}

	#region InputSystem Function
	public void OnMove(InputValue value)
	{
		if (!photonView.IsMine) return;
		MoveDir = value.Get<Vector2>();
		Model.SetMoving(MoveDir != Vector2.zero);

		if (MoveDir != Vector2.zero) _lastDir = MoveDir;

		// 두 키를 뗐을 때
		if (MoveDir == Vector2.zero)
		{
			bool isFound = false;
			foreach (var dir in _moveHistory.Reverse())
			{
				if (math.abs(dir.x) > 0.1f && math.abs(dir.y) > 0.1f)
				{
					_lastDir = dir;
					isFound = true;
					break;
				}
			}
			if (isFound) return;
			foreach (var dir in _moveHistory.Reverse())
			{
				if (dir != Vector2.zero)
				{
					_lastDir = dir;
					break;
				}
			}
			_moveHistory.Clear();
		}
	}

	public void OnSkill(SkillSlot slot, InputAction.CallbackContext ctx)
	{
		if (!photonView.IsMine) return;

		switch (ctx.phase)
		{
			case InputActionPhase.Started:
				//Debug.Log($"스킬 {slot} 키다운");
				IAttack attack = SkillCheck(slot, out var skill);
				if (attack == null || skill == null) return;
				if (skill.SkillAnimType == SkillAnimType.SpeAttack) View.SetIsSpeAttack();
				else View.SetIsAttack();
				Model.SetSkillCooldown(slot, skill.Cooldown);
				attack.Attack(transform, _lastDir, BattleData, skill);
				break;

			case InputActionPhase.Canceled:
				//Debug.Log($"스킬 {slot} 키업 : {ctx.duration}");
				break;
		}
	}
	#endregion

	public void StartPokeEvolution()
	{
		if (!photonView.IsMine) return;

		PokemonData nextPokeData = Model.GetNextEvoData();

		if (nextPokeData != null)
		{
			photonView.RPC(nameof(RPC_PokemonEvolution), RpcTarget.All, nextPokeData.PokeNumber);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!photonView.IsMine) return;
		photonView.RPC(nameof(RPC_SyncToNewPlayer), newPlayer, Model.PokeData.PokeNumber, Model.PokeLevel, Model.CurrentHp);
	}

	public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
	{
		if (Model.CurrentHp <= 0 || Model.IsDead) return false;

		BattleDataTable defenderData = ((IDamagable)this).BattleData;
		int damage = PokeUtils.CalculateDamage(attackerData, defenderData, skill);
		Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{defenderData.Level} {defenderData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");

		// 플레이어들과 AI를 구분해야함
		if (!attackerData.IsAI)
		{
			PlayerManager.Instance?.ShowDamageText(transform, damage, Color.white);
			ActionRPC(nameof(RPC_SetLastAttacker), this.photonView.Owner, attackerData.PC.photonView.ViewID);
		}
		else if (attackerData.IsAI)
		{
			ActionRPC(nameof(RPC_SetLastAttacker), this.photonView.Owner, -1);
		}

		ActionRPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
		return true;
	}

	IAttack SkillCheck(SkillSlot slot, out PokemonSkill skill)
	{
		skill = Model.GetSkill((int)slot);
		if (skill == null)
		{
			Debug.LogWarning("사용할 수 있는 스킬이 없습니다.");
			return null;
		}
		if (Model.IsSkillCooldown(slot))
		{
			Debug.LogWarning("스킬이 쿨타임입니다.");
			return null;
		}
		IAttack attack = new SkillStrategyAttack(skill.SkillName);
		if (attack == null)
		{
			Debug.LogWarning("정의되지 않은 스킬입니다.");
			return null;
		}
		return attack;
	}

	public void AddExp(int value)
	{
		Model.AddExp(value);
		PlayerManager.Instance.ShowDamageText(transform, $"+EXP {value}", Color.blue);
	}
	public void ActionRPC(string funcName, RpcTarget target, params object[] value) => photonView.RPC(funcName, target, value);
	public void ActionRPC(string funcName, Player targetPlayer, params object[] value) => photonView.RPC(funcName, targetPlayer, value);

	[PunRPC]
	public void RPC_PokemonEvolution(int pokeNumber, PhotonMessageInfo info)
	{
		PokemonData pokeData = Define.GetPokeData(pokeNumber);
		Model.PokemonEvolution(pokeData);
		View.SetAnimator(pokeData.AnimController);
		// TODO : 진화 연출
	}
	[PunRPC]
	public void RPC_ChangePokemonData(int pokeNumber)
	{
		var pokeData = Define.GetPokeData(pokeNumber);
		Model = new PlayerModel(Model.PlayerName, pokeData);
		View?.SetAnimator(pokeData.AnimController);
		if (PhotonNetwork.LocalPlayer.IsLocal) OnModelChanged?.Invoke(Model);
	}
	[PunRPC]
	public void RPC_CurrentHpChanged(int value)
	{
		if (!photonView.IsMine) Model.SetCurrentHp(value);
	}
	[PunRPC]
	public void RPC_LevelChanged(int value, PhotonMessageInfo info)
	{
		if (!photonView.IsMine)
		{
			Model.SetLevel(value);
		}
		else
		{
			var currentData = Model.PokeData;
			var nextData = Model.PokeData.NextEvoData;
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
		if (value > 0) View.SetIsHit();
		Debug.Log($"{value} 대미지 입음");
		if (photonView.IsMine)
		{
			Model.SetCurrentHp(Model.CurrentHp - value);
		}
		PlayerManager.Instance.ShowDamageText(transform, value, Color.red);
	}
	[PunRPC]
	public void RPC_SyncToNewPlayer(int pokeNumber, int level, int currentHp)
	{
		Debug.Log("새로운 플레이어 입장");
		var pokeData = Define.GetPokeData(pokeNumber);
		Model = new PlayerModel(Model.PlayerName, pokeData, level, 0, currentHp);
		Rank = new PokeRankHandler(this, Model);
		View?.SetAnimator(pokeData.AnimController);

		if (PhotonNetwork.LocalPlayer.IsLocal) OnModelChanged?.Invoke(Model);
	}
	[PunRPC]
	public void RPC_PlayerSetActive(bool value)
	{
		gameObject.SetActive(value);
	}
	[PunRPC]
	public void RPC_SetLastAttacker(int viewId)
	{
		if (!photonView.IsMine) return;
		if (viewId < 0) _lastAttacker = null;

		var pv = PhotonView.Find(viewId);
		if (pv != null) _lastAttacker = pv.GetComponent<PlayerController>();
	}
	[PunRPC]
	public void RPC_AddKillCount()
	{
		Debug.Log($"킬 증가! 현재 킬 : {KillCount}");
		KillCount++;
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
			pc.Rank = new PokeRankHandler(pc, pc.Model);
		}
		StatType statType = (StatType)statTypeIndex;
		Debug.Log($"{viewId} : [{statType} : {value}] 동기화 시작");
		pc.Rank.RankSync(statType, value);
	}
	////////////////////////////////// 스탯 변경

	public void ApplyStat(ItemData item)
	{
		Debug.Log($"{item.itemName} 획득!");
		switch (item.itemType)
		{
			case ItemType.Heal:
				Debug.Log($"{item.value} 회복");
				Model.SetHeal((int)item.value);
				break;
			case ItemType.LevelUp:
				Debug.Log($"플레이어 레벨 상승 {Model.PokeLevel} -> {Model.PokeLevel + 1}");
				Model.SetLevel(Model.PokeLevel + 1);
				break;
			case ItemType.Buff:
				Debug.Log($"TODO : 도구, 열매 등 스탯을 제외한 버프 획득");
				break;
			case ItemType.StatBuff:
				Debug.Log($"{item.affectedStat} 랭크 상승");
				Rank?.SetRank(item.affectedStat, (int)item.value, item.duration);
				break;
		}
	}

	public void RemoveStat(ItemData item)
	{
		// TODO : ApplyStat 적용 구조에 따라 수정하기
	}
}
