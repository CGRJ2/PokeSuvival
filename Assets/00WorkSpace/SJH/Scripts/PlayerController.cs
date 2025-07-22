using Photon.Pun;
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

		NetworkManager_SJH.Instance.PlayerFollowCam.Follow = transform;
	}


	void MoveInput()
	{
		//MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
		//MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
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

	public void OnMove(InputValue value)
	{
		if (!photonView.IsMine) return;
		MoveDir = value.Get<Vector2>();
	}
}
