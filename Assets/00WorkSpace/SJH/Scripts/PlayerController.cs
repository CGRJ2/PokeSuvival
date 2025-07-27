using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback, IDamagable
{
	[SerializeField] private PlayerModel _model;
	[SerializeField] private PlayerView _view;
	[SerializeField] private PlayerInput _input;
	[SerializeField] private bool _flipX;

	[field: SerializeField] public Vector2 MoveDir { get; private set; }
	BattleDataTable IDamagable.BattleData { get => new BattleDataTable(_model.PokeLevel, _model.PokeData, _model.AllStat, _model.MaxHp, _model.CurrentHp); }

	public int Test_Level;

	private int _maxLogCount = 10;
	[SerializeField] private Queue<Vector2> _moveHistory = new();
	[SerializeField] private Vector2 _lastDir = Vector2.down;

	void Awake()
	{
		_view = GetComponent<PlayerView>();
		_input = GetComponent<PlayerInput>();

		_moveHistory = new(_maxLogCount);
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			_model.SetLevel(Test_Level);
		}
	}

	public void PlayerInit(PokemonData pokeData)
	{
		Debug.Log("플레이어 초기화");

		ActionRPC(nameof(RPC_ChangePokemonData), RpcTarget.All, pokeData.PokeNumber);

		// TODO : 스킬 클래스로 분리
		SkillInit();

		PlayerManager.Instance.PlayerFollowCam.Follow = transform;

		_model.OnCurrentHpChanged += (hp) => { ActionRPC(nameof(RPC_CurrentHpChanged), RpcTarget.All, hp); };
		_model.OnPokeLevelChanged += (level) => { ActionRPC(nameof(RPC_LevelChanged), RpcTarget.All, level); };

		// TODO : 테스트 코드
		GameObject.Find("Button1").GetComponent<Button>().onClick.AddListener(() => { StartPokeEvolution(); });
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
			_view.SetFlip(_flipX = (bool)stream.ReceiveNext());
			// TODO : 실시간 동기화
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!photonView.IsMine)
		{
			gameObject.tag = "Enemy";
			return;
		}

		Debug.Log("플레이어 인스턴스화");

		gameObject.tag = "Player";
		object[] data = photonView.InstantiationData;
		PokemonData pokeData = null;
		if (data[0] is int pokeNumber) pokeData = Define.GetPokeData(pokeNumber);
		else if (data[0] is string pokeName) pokeData = Define.GetPokeData(pokeName);

		PlayerInit(pokeData);
	}

	void SkillInit()
	{
		for (int i = 1; i <= 4; i++)
		{
			int slotIndex = i;
			var action = _input.actions[$"Skill{slotIndex}"];
			action.started += ctx => OnSkill((SkillSlot)slotIndex - 1, ctx);
			action.canceled += ctx => OnSkill((SkillSlot)slotIndex - 1, ctx);
		}
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
		_view.PlayerMove(MoveDir, _lastDir, _model.GetMoveSpeed());
	}

	#region InputSystem Function
	public void OnMove(InputValue value)
	{
		if (!photonView.IsMine) return;
		MoveDir = value.Get<Vector2>();
		_model.SetMoving(MoveDir != Vector2.zero);

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
				if (attack == null) return;
				IDamagable damagable = this;
				attack.Attack(transform, _lastDir, damagable.BattleData, skill);
				_model.SetSkillCooldown(slot);

				// TODO : 모델 처리
				// TODO : 뷰 처리
				break;
			case InputActionPhase.Canceled:
				//Debug.Log($"스킬 {slot} 키업 : {ctx.duration}");
				// TODO : 모델 처리
				// TODO : 뷰 처리
				break;
		}
	}
	#endregion

	public void StartPokeEvolution()
	{
		if (!photonView.IsMine) return;

		PokemonData nextPokeData = _model.GetNextEvoData();

		if (nextPokeData != null)
		{
			photonView.RPC(nameof(RPC_PokemonEvolution), RpcTarget.All, nextPokeData.PokeNumber);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!photonView.IsMine) return;
		photonView.RPC(nameof(RPC_SyncToNewPlayer), newPlayer, _model.PokeData.PokeNumber, _model.PokeLevel, _model.CurrentHp);
	}

	public void TakeDamage(int value) => ActionRPC(nameof(RPC_TakeDamage), RpcTarget.All, value);

	IAttack SkillCheck(SkillSlot slot, out PokemonSkill skill)
	{
		skill = _model.GetSkill((int)slot);
		if (skill == null)
		{
			Debug.Log("사용할 수 있는 스킬이 없습니다.");
			return null;
		}
		if (_model.IsSkillCooldown(slot, skill.Cooldown))
		{
			Debug.Log("스킬이 쿨타임입니다.");
			return null;
		}
		IAttack attack = null;
		switch (skill.AttackType)
		{
			case AttackType.Melee: attack = new MeleeAttack(); break;
			case AttackType.Ranged: attack = new RangedAttack(); break;
		}
		if (attack == null)
		{
			Debug.Log("정의되지 않은 스킬입니다.");
			return null;
		}
		return attack;
	}

	void ActionRPC(string funcName, RpcTarget target, object value) => photonView.RPC(funcName, target, value);

	[PunRPC]
	public void RPC_PokemonEvolution(int pokeNumber, PhotonMessageInfo info)
	{
		Debug.Log("RPC_PokemonEvolution");
		PokemonData pokeData = Define.GetPokeData(pokeNumber);
		_model.PokemonEvolution(pokeData);
		_view.SetAnimator(pokeData.AnimController);
		// TODO : 진화 연출
	}
	[PunRPC]
	public void RPC_ChangePokemonData(int pokeNumber)
	{
		var pokeData = Define.GetPokeData(pokeNumber);
		_model = new PlayerModel(_model.PlayerName, pokeData);
		_view?.SetAnimator(pokeData.AnimController);
	}
	[PunRPC]
	public void RPC_CurrentHpChanged(int value)
	{
		if (!photonView.IsMine) _model.SetCurrentHp(value);
	}
	[PunRPC]
	public void RPC_LevelChanged(int value, PhotonMessageInfo info)
	{
		Debug.Log($"RPC_LevelChanged");
		if (!photonView.IsMine)
		{
			_model.SetLevel(value);

		}
		else
		{
			var currentData = _model.PokeData;
			var nextData = _model.PokeData.NextEvoData;
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
		Debug.Log($"{value} 대미지 입음");
		if (photonView.IsMine)
		{
			_model.SetCurrentHp(_model.CurrentHp - value);
		}
		PlayerManager.Instance.ShowDamageText(transform, value, Color.red);
		// TODO : RPC All View 피격 연출
	}
	[PunRPC]
	public void RPC_SyncToNewPlayer(int pokeNumber, int level, int currentHp)
	{
		Debug.Log("새로운 플레이어 입장");
		var pokeData = Define.GetPokeData(pokeNumber);
		_model = new PlayerModel(_model.PlayerName, pokeData, level, 0, currentHp);
		_view?.SetAnimator(pokeData.AnimController);
	}
}
