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
	}

	public void PlayerInstaniate()
	{
		string pokemonName = (string)PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"];
		Debug.Log(pokemonName);
		// TODO : 아이템 아이디로 지닌 도구 정보 가져오기
		PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0,
				new object[]
				{
					pokemonName
				});

		// test
		if (!PhotonNetwork.IsMasterClient) return;
		PhotonNetwork.InstantiateRoomObject("Enemy", new Vector3(100f, 100f), Quaternion.identity, 0,
			new object[]
			{
				7,	// pokeNumber
				3	// level
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
        UIManager.Instance.InGameGroup.GameOverViewUpdate(LocalPlayerController);
    }
    IEnumerator PlayerDeadRoutine(int totalExp)
	{
		Debug.Log("플레이어 사망 > 로비로 이동");
		_playerFollowCam.Follow = null;
		yield return new WaitForSeconds(_objectDeleteTime);
		LocalPlayerController.RPC.ActionRPC(nameof(LocalPlayerController.RPC.RPC_PlayerSetActive), RpcTarget.AllBuffered, false);
	}
	public void PlayerToLobby()
	{
		StopPlayerRoutine();
		LocalPlayerController.DisconnectSkillEvent();
	}

	public void PlayerRespawn()
	{
		StopPlayerRoutine();
		LocalPlayerController.PlayerRespawn();
	}

	void StopPlayerRoutine()
	{
		if (_playerDeleteRoutine != null) StopCoroutine(_playerDeleteRoutine);
		_playerDeleteRoutine = null;
	}
}
