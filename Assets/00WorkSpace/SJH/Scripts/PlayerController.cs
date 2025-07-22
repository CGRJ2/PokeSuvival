using UnityEngine;

public class PlayerController : MonoBehaviour // TODO : 포톤 추가되면 연결 MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private PlayerModel _model;
	[SerializeField] private PlayerView _view;

	public Vector2 MoveDir;

	void Awake()
	{
		_model = new PlayerModel("Test", 5);
		_view = GetComponent<PlayerView>();
	}

	void Update()
	{
		MoveInput();
	}

	public void PlayerInit()
	{
		// TODO : 포톤뷰 IsMine에 따라 카메라 활성화 비활성화
	}


	void MoveInput()
	{
		MoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
		_view.Move(MoveDir, _model.MoveSpeed);
	}
}
