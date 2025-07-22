using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private PlayerModel _model;
	[SerializeField] private PlayerView _view;

	public Vector2 MoveDir;

	void Update()
	{
		if (!photonView.IsMine) return;

		MoveInput();
	}

	public void PlayerInit(PokemonData pokeData)
	{
		Debug.Log("플레이어 초기화");
		// TODO : 포톤뷰 IsMine에 따라 카메라 활성화 비활성화
		// 시네머신 카메라
		_model = new PlayerModel("Test", pokeData);
		_view = GetComponent<PlayerView>();
		_view.SetAnimator(pokeData.AnimController);

		//Camera.main.transform.SetParent(transform);
		NetworkManager_SJH.Instance.PlayerFollowCam.Follow = transform;
	}


	void MoveInput()
	{
		MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
		_view.Move(MoveDir, _model.PokeData.BaseStat.GetMoveSpeed());
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!photonView.IsMine) return;

		Debug.Log("플레이어 인스턴스화");
		// TODO : 포켓몬데이터 클래스를 어떻게 받을지

		object[] data = photonView.InstantiationData;
		PokemonData pokeData = null;
		if (data[0] is int pokeNumber) pokeData = Define.GetPokeData(pokeNumber);
		else if (data[0] is string pokeName) pokeData = Define.GetPokeData(pokeName);

		PlayerInit(pokeData);
	}
}
