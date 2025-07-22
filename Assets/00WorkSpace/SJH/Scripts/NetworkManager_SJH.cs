using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager_SJH : NetworkManager
{
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
