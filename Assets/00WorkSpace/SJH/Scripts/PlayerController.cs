﻿using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private PlayerModel _model;
	[SerializeField] private PlayerView _view;
	[SerializeField] private PlayerInput _input;
	[SerializeField] private bool _flipX;
	
	public Vector2 MoveDir { get; private set; }

	void Awake()
	{
		_view = GetComponent<PlayerView>();
		_input = GetComponent<PlayerInput>();
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();
	}

	public void PlayerInit(PokemonData pokeData)
	{
		Debug.Log("플레이어 초기화");

		//ChangePokemonData(pokeData);
		photonView.RPC("RPC_ChangePokemonData", RpcTarget.All, pokeData.PokeNumber);

		// TODO : 스킬 클래스로 분리
		SkillInit();

		NetworkManager_SJH.InstanceTest.PlayerFollowCam.Follow = transform;

		// TODO : 테스트 코드
		GameObject.Find("Button1").GetComponent<Button>().onClick.AddListener(() => { StartPokeEvolution(); });
		//GameObject.Find("Button1").GetComponent<Button>().onClick.AddListener(() => { ChangePokemonData(Define.GetPokeData(1)); });
		//GameObject.Find("Button4").GetComponent<Button>().onClick.AddListener(() => { ChangePokemonData(Define.GetPokeData(4)); });
	}

	public void ChangePokemonData(PokemonData pokeData)
	{
		if (!photonView.IsMine) return;

		photonView.RPC("RPC_ChangePokemonData", RpcTarget.All, pokeData.PokeNumber);
	}

	[PunRPC]
	public void RPC_ChangePokemonData(int pokeNumber)
	{
		var pokeData = Define.GetPokeData(pokeNumber);
		_model = new PlayerModel(_model.PlayerName, pokeData);
		//if (_view == null) _view = GetComponent<PlayerView>();
		_view.SetAnimator(pokeData.AnimController);
	}

	void MoveInput()
	{
		if (MoveDir.x != 0) _flipX = MoveDir.x > 0.1f;
		_view.PlayerMove(MoveDir, _model.GetMoveSpeed());
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
		if (stream.IsWriting)
		{
			stream.SendNext(_flipX);
		}
		else
		{
			_flipX = (bool)stream.ReceiveNext();
			_view.SetFlip(_flipX);
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!photonView.IsMine) return;

		Debug.Log("플레이어 인스턴스화");

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
			action.started += ctx => OnSkill((SkillSlot)slotIndex, ctx);
			action.canceled += ctx => OnSkill((SkillSlot)slotIndex, ctx);
		}
	}

	#region InputSystem Function
	public void OnMove(InputValue value)
	{
		if (!photonView.IsMine) return;
		MoveDir = value.Get<Vector2>();
		_model.SetMoving(MoveDir != Vector2.zero);
	}

	public void OnSkill(SkillSlot slot, InputAction.CallbackContext ctx)
	{
		if (!photonView.IsMine) return;

		switch (ctx.phase)
		{
			case InputActionPhase.Started:
				Debug.Log($"스킬 {(int)slot} 사용");
				// TODO : 모델 처리
				// TODO : 뷰 처리
				break;
			case InputActionPhase.Canceled:
				Debug.Log($"스킬 {(int)slot} 완료 : {ctx.duration}");
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
			photonView.RPC("RPC_PokemonEvolution", RpcTarget.All, nextPokeData.PokeNumber);
		}
	}

	[PunRPC]
	public void RPC_PokemonEvolution(int pokeNumber)
	{
		PokemonData pokeData = Define.GetPokeData(pokeNumber);
		if (pokeData == null) return;
		int currentLevel = _model.PokeLevel;
		int currentExp = _model.PokeExp;

		_model = new PlayerModel(_model.PlayerName, pokeData);

		if (currentLevel > 1)
		{
			_model.SetLevel(currentLevel);
			_model.SetExperience(currentExp);
		}
		
		_view.SetAnimator(pokeData.AnimController);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!photonView.IsMine) return;

		photonView.RPC("RPC_ChangePokemonData", newPlayer, _model.PokeData.PokeNumber);
	}

	//public float Health { get => health; set => ActionPRC("", health); }

	//private void ActionPRC(string functionName, object value)
	//{
	//	photonView.RPC(functionName, RpcTarget.All, value);
	//}
}
