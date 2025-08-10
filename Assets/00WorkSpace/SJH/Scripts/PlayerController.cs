using NTJ;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback, IDamagable, IStatReceiver
{
	#region Field
	[field: SerializeField] public PlayerModel Model { get; private set; }
	[field: SerializeField] public PlayerView View { get; private set; }
	[field: SerializeField] public PokeRankHandler Rank { get; private set; }
	[field: SerializeField] public NetworkHandler RPC { get; private set; }
	[field: SerializeField] public PokeStatusHandler Status { get; private set; }
	[field: SerializeField] public PokeBuffHandler Buff { get; private set; }
	[field: SerializeField] public StatusController SC { get; private set; }

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
				Debug.Log("Rank == null 초기화 시작");
				Rank = new PokeRankHandler(this, Model);
				ConnectRankEvent();
			}
			if (Status == null || Status.BaseData == null)
			{
				Debug.Log("Status == null 초기화 시작");
				Status = new PokeStatusHandler(this, Model);
			}
			if (Buff == null || Buff.BaseData == null)
			{
				Debug.Log("Buff == null 초기화 시작");
				Buff = new PokeBuffHandler(this, Model);
				ConnectBuffEvent();
			}
			var newStat = Rank.GetRankedStat();
			if (!_prevRankedStat.IsEqual(newStat))
			{
				_prevRankedStat = newStat;
				_battleData = new BattleDataTable(Model.PokeLevel, Model.PokeData, _prevRankedStat, Model.MaxHp, Model.CurrentHp,
					false, this, _heldItem, Status.CurrentStatus, Buff.CurrentBuffs);
			}
			return _battleData;
		}
		private set { _battleData = value; }
	}
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
	[field: SerializeField] public int KillCount { get; private set; }
	[field: SerializeField] public PlayerController LastAttacker { get; private set; }

	// 이동 제어
	// TODO : 스테이터스 핸들러에서 바인딩, 얼음 두가지 추가해서 제어

	// 지닌 도구
	[SerializeField] private ItemPassive _heldItem;
	public ItemPassive HeldItem { get => _heldItem; private set { _heldItem = value; _prevRankedStat = default; } }

	// 버프 획득용 이벤트 // 랭크업, 버프, 아이템버프, 디버프 등
	public Action<Sprite, float> OnBuffUpdate;

	// 오디오 클립
	[SerializeField] private AudioClip[] _hitClips = new AudioClip[3];
	[SerializeField] private AudioSource _audio;

	// 경험치 구슬 획득 SFX
	[SerializeField] private AudioSource _audio_GetEXP;
	
	// 레벨업 SFX
	public AudioSource _audio_LevelUp;

    // 스탯 상승 하락 SFX
    [SerializeField] private AudioSource _audio_StatRise;
    [SerializeField] private AudioSource _audio_StatFall;

    // 회복 SFX
    [SerializeField] private AudioSource _audio_GetHeal;

	// 도구 사용 SFX
    [SerializeField] private AudioSource _audio_ItemActive;

	// 기절 SFX
    [SerializeField] private AudioSource _audio_Faint;

    #endregion

    public int Test_Level; // TODO : 변화할 레벨 스페이스바로 레벨 변경 나중에 삭제 

	void Awake()
	{
		View = GetComponent<PlayerView>();
		_input = GetComponent<PlayerInput>();
		RPC = GetComponent<NetworkHandler>();
		_audio = GetComponent<AudioSource>();
		SC = GetComponentInChildren<StatusController>();

		_moveHistory = new(_maxLogCount);
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();

		// TODO : 테스트 함수 변수랑 같이 삭제
		//if (Input.GetKeyDown(KeyCode.Space))
		//{
		//	Model.SetLevel(Test_Level);
		//}
		//if (Input.GetKeyDown(KeyCode.B)) Status?.RemoveStatus(StatusType.Freeze);
		//if (Input.GetKeyDown(KeyCode.N)) Status?.StatusAllClear();
	}
	void MoveInput()
	{
		if (Status == null) Status = new PokeStatusHandler(this, Model);

		// 동상 : 이동 불가
		if (Status.IsFreeze() || Status.IsSleep() || Status.IsStun())
		{
			View.SetStop();
			return;
		}

		if (MoveDir.x != 0) _flipX = MoveDir.x > 0.1f;

		if (MoveDir != Vector2.zero)
		{
			_moveHistory.Enqueue(MoveDir);
			if (_moveHistory.Count > _maxLogCount) _moveHistory.Dequeue();
		}

		// 속박 : 방향만 전환
		if (Status.IsBinding())
		{
			View.PlayerMove(MoveDir, _lastDir, 0);
		}
		else
		{
			// 마비 : 속도 반감
			float speed = Model.GetMoveSpeed();
			if (Status.IsParalysis()) speed *= 0.5f;
			View.PlayerMove(MoveDir, _lastDir, speed);
		}
	}

	#region Player Init, Respawn
	public void PlayerInit()
	{
		Debug.Log("플레이어 초기화");
		PlayerSetting(false);
	}

	public void PlayerRespawn()
	{
		Debug.Log("플레이어 리스폰");
		PlayerSetting(true);
	}
	void PlayerSetting(bool isRespawn)
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
		LastAttacker = null;

		// 랜덤위치 스폰
		//transform.position = ExpOrbSpawner.Instance.GetRandomTilePosition();

		PokemonData pokeData = null;

		// 플레이어의 커스텀프로퍼티로 사용할 포켓몬 지정
		if (isRespawn && PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("StartingPokemon", out var startPoke) && startPoke is string pokemonName) pokeData = Define.GetPokeData(pokemonName);

		if (pokeData == null)
		{
			object[] data = photonView.InstantiationData;
			if (data[0] is int pokeNumber) pokeData = Define.GetPokeData(pokeNumber);
			else if (data[0] is string pokeName) pokeData = Define.GetPokeData(pokeName);
		}

		// 아이템 장착
		object[] datas = photonView.InstantiationData;
		if (datas.Length > 1 && datas[1] is int itemId && itemId > 0 && ItemDataManager.Instance.GetItemById(itemId) is ItemPassive heldItem)
		{
			HeldItem = heldItem;
			OnBuffUpdate?.Invoke(HeldItem.sprite, -99f);
		}

		RPC.ActionRPC(nameof(RPC.RPC_ChangePokemonData), RpcTarget.All, PhotonNetwork.NickName, NetworkManager.Instance.GetUserId(), pokeData.PokeNumber);

		PlayerManager.Instance.PlayerFollowCam.Follow = transform;
		PlayerManager.Instance.LocalPlayerController = this;

		if (isRespawn)
		{
			_input.actions.Enable();
			View.SetIsDead(false);
			RPC.ActionRPC(nameof(RPC.RPC_PlayerSetActive), RpcTarget.AllBuffered, true);
		}

		ConnectEvent();
		OnModelChanged?.Invoke(Model);
	}
	#endregion

	#region Photon Callback
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
		if (stream.IsWriting) stream.SendNext(_flipX);
		else View.SetFlip(_flipX = (bool)stream.ReceiveNext());
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
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!photonView.IsMine) return;
		// 1. B 클라이언트 입장
		// 2. 이미 접속해있는 로컬 클라이언트(A)의 오브젝트에서 실행
		// 3. B 클라이언트에 있는 A 오브젝트의 포톤함수 실행으로 동기화 (B클라에서 A의 이름, 포켓몬, 레벨, 체력 동기화)
		// 4. 하지만 A 클라이언트에서 B 오브젝트의 이름은 동기화가 안됨
		RPC.ActionRPC(nameof(RPC.RPC_SyncToNewPlayer), newPlayer, Model.PlayerName, Model.UserId, Model.PokeData.PokeNumber, Model.PokeLevel, Model.CurrentHp);
		//photonView.RPC(nameof(RPC.RPC_SyncToNewPlayer), newPlayer, Model.PlayerName, Model.UserId, Model.PokeData.PokeNumber, Model.PokeLevel, Model.CurrentHp);
	}
	#endregion

	#region Event Function
	public void ConnectEvent()
	{
		DisconnectEvent();

		Model.OnCurrentHpChanged += (hp) =>
		{
			RPC.ActionRPC(nameof(RPC.RPC_CurrentHpChanged), RpcTarget.All, hp);
		};
		Model.OnDied += () =>
		{
			// 사망 시간 기록
			_endTime = Time.time;
			// 막타친 플레이어 킬카운트 증가
			if (LastAttacker != null)
			{
				Debug.Log($"{LastAttacker.Model.PokeData.PokeName} 의 킬 카운트 증가 시도");
				/*	A -> B 를 죽였을 때
				 *	B의 포톤뷰에서 A의 RPC_AddKillCount 함수를 실행하는게 아니라
				 *	실행을 보낸 B의 포톤뷰에서 RPC_AddKillCount 함수를 실행
				 *	
				 *	그래서 보낼 때 A 포톤뷰에서 RPC를 해야함
				 */
				//RPC.ActionRPC(nameof(RPC.RPC_AddKillCount), LastAttacker.photonView.Owner);
				LastAttacker.photonView.RPC(nameof(RPC.RPC_AddKillCount), LastAttacker.photonView.Owner);

				// 랭크 초기화
				Rank?.RankAllClear();
				// 상태 초기화
				Status?.StatusAllClear();
				// 버프 초기화
				Buff?.BuffAllClear();

				// 총 경험치의 일부분 드랍
				RPC.ActionRPC(nameof(RPC.RPC_PlayerDead), RpcTarget.AllBuffered, Model.GetDeathExp());
			}

			_input.actions.Disable();
			View.SetIsDead(true);
			PlayerManager.Instance.PlayerDead(Model.TotalExp);
			_audio_Faint.Play();
			Debug.LogWarning("플레이어 사망");
		};
		Model.OnPokeLevelChanged += (level) => { RPC.ActionRPC(nameof(RPC.RPC_LevelChanged), RpcTarget.All, level); };
		OnModelChanged += (model) =>
		{
			if (model != null && Rank == null) Rank = new PokeRankHandler(this, model);
			if (model != null && Status == null) Status = new PokeStatusHandler(this, model);
			if (model != null && Buff == null) Buff = new PokeBuffHandler(this, model);
			ConnectRankEvent();
			ConnectBuffEvent();
			UIManager.Instance.InGameGroup.UpdateSkillSlots(model);
		};
		Model.OnTotalExpChanged += (exp) => { RPC.ActionRPC(nameof(RPC.RPC_TotalExpChanged), RpcTarget.All, exp); };

		ConnectSkillEvent();
		ConnectRankEvent();
		ConnectBuffEvent();
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

		if (Rank.OnRankChanged == null) Rank.OnRankChanged += (statType, prev, next) =>
		{
			// 스탯종류, 이전값, 이후값
		};

		if (Rank.OnSyncToRank == null) Rank.OnSyncToRank += (statType, next) =>
		{
			if (!photonView.IsMine) return;
			// 스탯종류, 최신값
			Debug.Log($"{Model.PokeData.PokeName} [{statType} : {next}] 동기화 시작");
			RPC.ActionRPC(nameof(RPC.RPC_RankSync), RpcTarget.OthersBuffered, photonView.ViewID, (int)statType, next);
		};
	}
	public void ConnectBuffEvent()
	{
		if (Buff == null) return;

		if (Buff.OnSyncToBuff == null) Buff.OnSyncToBuff += (skillName) =>
		{
			if (!photonView.IsMine) return;
			Debug.Log($"{Model.PokeData.PokeName} [{skillName} 버프] 동기화 시작");
			RPC.ActionRPC(nameof(RPC.RPC_BuffSync), RpcTarget.OthersBuffered, photonView.ViewID, skillName);
		};
	}

	public void DisconnectRankEvent()
	{
		if (Rank == null) return;

		Rank.OnRankChanged = null;
		Rank.OnSyncToRank = null;
	}
	#endregion

	#region InputSystem Function
	public void OnMove(InputValue value)
	{
		if (!photonView.IsMine) return;
		MoveDir = value.Get<Vector2>();
		Model.SetMoving(MoveDir != Vector2.zero);

		if (Status != null && Status.IsConfusion()) MoveDir *= -1f;

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
		if (Status.IsFreeze() || Status.IsSleep() || Status.IsStun()) return;
		switch (ctx.phase)
		{
			case InputActionPhase.Started:
				//Debug.Log($"스킬 {slot} 키다운");
				Attack(slot);
				break;

			case InputActionPhase.Canceled:
				//Debug.Log($"스킬 {slot} 키업 : {ctx.duration}");
				break;
		}
	}
	#endregion
	[SerializeField] private bool isFuryAttack = false;	
	#region Battle Attack, SkillCheck, TakeDamage, StatusCheck
	public void Attack(SkillSlot slot)
	{
		IAttack attack = SkillCheck(slot, out var skill);
		if (attack == null || skill == null) return;
		if (skill.SkillAnimType == SkillAnimType.SpeAttack) View.SetIsSpeAttack();
		else View.SetIsAttack();

		if (skill.SkillName == "연속자르기")
		{
			// 첫 공격
			if (!isFuryAttack)
			{
				Model.SetSkillCooldown(slot, 2f);
				isFuryAttack = true;
			}
			else
			{
				Model.SetSkillCooldown(slot, skill.Cooldown);
				isFuryAttack = false;
			}
		}
		else Model.SetSkillCooldown(slot, skill.Cooldown);

		attack.Attack(transform, _lastDir, BattleData, skill);
	}
	IAttack SkillCheck(SkillSlot slot, out PokemonSkill skill)
	{
		skill = Model.GetSkill((int)slot);
		if (skill == null) return null;
		if (Model.IsSkillCooldown(slot)) return null;
		IAttack attack = new SkillStrategyAttack(skill.SkillName);
		if (attack == null) return null;
		return attack;
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
			RPC.ActionRPC(nameof(RPC.RPC_SetLastAttacker), this.photonView.Owner, attackerData.PC.photonView.ViewID);
		}
		else if (attackerData.IsAI)
		{
			RPC.ActionRPC(nameof(RPC.RPC_SetLastAttacker), this.photonView.Owner, -1);
		}

		// 상태이상
		if (Status == null) Status = new PokeStatusHandler(this, Model);
		if (Status.SetStatus(skill)) RPC.ActionRPC(nameof(RPC.RPC_SetStatus), RpcTarget.OthersBuffered, skill.SkillName);

		int soundIndex = 1;
		float typeDamage = PokeUtils.GetTypeBonus(skill.PokeType, Model.PokeData.PokeTypes);
		if (typeDamage < 1) soundIndex = 0;
		else if (typeDamage == 1) soundIndex = 1;
		else if (typeDamage > 1) soundIndex = 2;

		// 대미지 처리
		RPC.ActionRPC(nameof(RPC.RPC_TakeDamage), RpcTarget.All, damage, soundIndex);
		return true;
	}
	#endregion

	#region Model, View, Rank, LastAttacker, KillCount setter
	public void SetModel(PlayerModel model)
	{
		Model = model;

		if (model != null && model.PokeData != null) SetRank(new PokeRankHandler(this, model));
		if (model != null && model.PokeData != null) SetStatus(new PokeStatusHandler(this, model));

		OnModelChanged?.Invoke(model);
	}
	public void SetView(PlayerView view) => View = view;
	public void SetRank(PokeRankHandler rank) => Rank = rank;
	public void SetStatus(PokeStatusHandler status) => Status = status;
	public void SetBuff(PokeBuffHandler buff) => Buff = buff;
	public void SetLastAttacker(PlayerController lastAttacker) => LastAttacker = lastAttacker;
	public void AddKillCount() => KillCount++;
	public void PlayHitSound(int hitIndex)
	{
		if (hitIndex < 0) hitIndex = 1;
		else if (hitIndex > _hitClips.Length) hitIndex = 1;

		var hitClip = _hitClips[hitIndex];
		_audio.clip = hitClip;
		_audio.Play();
	}
	#endregion

	#region Interact AddExp, ApplyStat, RemoveStat
	public void AddExp(int value)
	{
		float adjustValue = 0.2f;
		int level = Model.PokeLevel;
		float up = (5f / 4f) * ((3 * (level * level)) + (3 * level) + 1);
		float down = 2f + Mathf.Floor(adjustValue * (level - 1));
		int exp = Mathf.FloorToInt(up / down);
		exp = Mathf.Max(value, exp);

		Model.AddExp(exp);
		PlayerManager.Instance.ShowDamageText(transform, $"+EXP {exp}", Color.blue);

		_audio_GetEXP.Play();
	}
	public void ApplyStat(ItemData item)
	{
        Debug.Log($"{item.itemName} 획득!");
		switch (item.itemType)
		{
			case ItemType.Heal:
				Debug.Log($"{item.value} 회복");
				Model.SetHeal((int)item.value);
                _audio_GetHeal.Play();
                break;
			case ItemType.LevelUp:
				Debug.Log($"플레이어 레벨 상승 {Model.PokeLevel} -> {Model.PokeLevel + 1}");
				Model.SetLevel(Model.PokeLevel + 1);
                _audio_LevelUp.Play();
                break;
			case ItemType.Buff:
				Debug.Log($"TODO : 도구, 열매 등 스탯을 제외한 버프 획득");
				// 랭크 아이템 획득시 이벤트 실행
				OnBuffUpdate?.Invoke(item.sprite, item.duration);
                _audio_ItemActive.Play();
                break;
			case ItemType.StatBuff:
				Debug.Log($"{item.affectedStat} 랭크 상승");
				// 랭크 아이템 획득시 이벤트 실행
				OnBuffUpdate?.Invoke(item.sprite, item.duration);
				Rank?.SetRank(item.affectedStat, (int)item.value, item.duration);
                _audio_StatRise.Play();
                break;
		}

    }

    public void RemoveStat(ItemData item)
	{
		// TODO : ApplyStat 적용 구조에 따라 수정하기
	}

	#endregion
}
