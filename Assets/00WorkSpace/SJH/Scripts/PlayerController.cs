using Photon.Pun;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private PlayerModel _model;
	[SerializeField] private PlayerView _view;
	[SerializeField] private PlayerInput _input;

	public Vector2 MoveDir;

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();
	}

	public void PlayerInit(PokemonData pokeData)
	{
		Debug.Log("플레이어 초기화");

		_model = new PlayerModel("Test", pokeData);

		_view = GetComponent<PlayerView>();
		_view.SetAnimator(pokeData.AnimController);

		_input = GetComponent<PlayerInput>();
		SkillInit();

		NetworkManager_SJH.Instance.PlayerFollowCam.Follow = transform;
	}


	void MoveInput()
	{
		_view.PlayerMove(MoveDir, _model.PokeData.BaseStat.GetMoveSpeed());
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
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
	}

	public void OnSkill(SkillSlot slot, InputAction.CallbackContext ctx)
	{
		if (!photonView.IsMine) return;
		switch (ctx.phase)
		{
			case InputActionPhase.Started:
				Debug.Log($"스킬 {(int)slot} 사용");
				break;
			case InputActionPhase.Canceled:
				Debug.Log($"스킬 {(int)slot} 완료 : {ctx.duration}");
				break;
		}
	}
	#endregion
}
