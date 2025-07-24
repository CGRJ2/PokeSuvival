using Photon.Pun;
using UnityEngine;
using Cinemachine;

public class NetworkManager_SJH : NetworkManager
{
	public static NetworkManager_SJH Instance { get; private set; }

	public CinemachineVirtualCamera PlayerFollowCam;

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
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("마스터 연결");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("방 입장");
		var player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0,
			new object[]
			{
				/*도감번호*/1
				/*or 이름*/
			});
	}
}
