using Cinemachine;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
	[SerializeField] private CinemachineVirtualCamera _playerFollowCam;
	[SerializeField] private GameObject _floatingTextPrefab;
	public PlayerController LocalPlayerController;
	public string LobbySceneName;
	[Tooltip("플레이어 시체가 사라지는 시간")]
	[SerializeField] private float _objectDeleteTime;

	public CinemachineVirtualCamera PlayerFollowCam
	{
		get
		{
			if (_playerFollowCam == null)
			{
				_playerFollowCam = FindObjectOfType<CinemachineVirtualCamera>();
			}
			return _playerFollowCam;
		}
		private set
		{
			_playerFollowCam = value;
		}
	}

	public static PlayerManager Instance { get; private set; }

	private Coroutine _playerDeleteRoutine;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		//PhotonNetwork.ConnectUsingSettings();
	}

    /*public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("마스터 연결 후 로비 상태로 변경");

		StartCoroutine(DelayedInit());
    }

	IEnumerator DelayedInit()
	{
		yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("로비 상태에서 룸 생성 or 입장 시도");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
	{
		Debug.Log("룸 입장");
		PlayerInstaniate();
	}*/

	public void PlayerInstaniate()
	{
		string pokemonName = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
		Debug.Log(pokemonName);

		PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0,
				new object[]
				{
					pokemonName
				});
	}

	public void ShowDamageText(Transform spawnPos, int damage, Color color)
	{
		if (_floatingTextPrefab.Equals(null)) return;

		var go = Instantiate(_floatingTextPrefab, spawnPos.position, Quaternion.identity);
		go.GetComponent<FloatingText>()?.InitFloatingDamage($"{damage}", color);
	}
	public void ShowDamageText(Transform spawnPos, string text, Color color)
	{
		if (_floatingTextPrefab.Equals(null)) return;

		var go = Instantiate(_floatingTextPrefab, spawnPos.position, Quaternion.identity);
		go.GetComponent<FloatingText>()?.InitFloatingDamage($"{text}", color);
	}

	public void PlayerDead(int totalExp)
	{
		// TODO : 사망 UI 활성화
		_playerDeleteRoutine = StartCoroutine(PlayerDeadRoutine(totalExp));
        UIManager.Instance.InGameGroup.GameOverViewUpdate(LocalPlayerController.Model);
    }
    IEnumerator PlayerDeadRoutine(int totalExp)
	{
		Debug.Log("플레이어 사망 > 로비로 이동");
		_playerFollowCam.Follow = null;
		yield return new WaitForSeconds(_objectDeleteTime);
		LocalPlayerController.ActionRPC(nameof(LocalPlayerController.RPC_PlayerSetActive), RpcTarget.AllBuffered, false);
	}
	public void PlayerToLobby()
	{
		LocalPlayerController.DisconnectSkillEvent();
	}

	public void PlayerRespawn()
	{
		StopCoroutine(_playerDeleteRoutine);
		_playerDeleteRoutine = null;

		LocalPlayerController.PlayerRespawn();
	}
}
