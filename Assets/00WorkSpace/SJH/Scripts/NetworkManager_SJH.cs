using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class NetworkManager_SJH : MonoBehaviourPunCallbacks
{
	public static NetworkManager_SJH InstanceTest { get; private set; }

	public CinemachineVirtualCamera PlayerFollowCam;

	void Awake()
	{
		if (InstanceTest == null)
		{
			InstanceTest = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		PhotonNetwork.ConnectUsingSettings(); // 
	}



	public override void OnConnectedToMaster()
	{
		Debug.Log("SJH_마스터 연결");
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
