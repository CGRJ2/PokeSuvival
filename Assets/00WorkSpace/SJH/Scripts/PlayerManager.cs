using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
	[SerializeField] private CinemachineVirtualCamera _playerFollowCam;
	[SerializeField] private GameObject _floatingTextPrefab;

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

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("SJH_마스터 연결");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("방 입장");
		int ran = Random.Range(0, 2);
		if (ran == 0)
		{
			Debug.Log("이상해씨 생성");
			var player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0,
				new object[]
				{
					/*도감번호*/1
					/*or 이름*/
				});
		}
		else
		{
			Debug.Log("파이리 생성");
			var player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0,
				new object[]
				{
					/*도감번호*/4
					/*or 이름*/
				});
		}
	}
	public void ShowDamageText(Transform spawnPos, int damage, Color color)
	{
		if (_floatingTextPrefab.Equals(null)) return;

		var go = Instantiate(_floatingTextPrefab, spawnPos.position, Quaternion.identity);
		go.GetComponent<FloatingText>()?.InitFloatingDamage($"{damage}", color);
	}
}
