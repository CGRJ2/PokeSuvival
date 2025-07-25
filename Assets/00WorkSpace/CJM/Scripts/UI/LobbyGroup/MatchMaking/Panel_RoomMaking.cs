using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_RoomMaking : MonoBehaviour
{
    [SerializeField] TMP_InputField tmp_RoomName;
    [SerializeField] Button btn_RoomCreate;

    public void Init()
    {
        btn_RoomCreate.onClick.AddListener(CreateRoom);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(tmp_RoomName.text))
        {
            Debug.LogError("방 이름이 올바르지 않습니다.");
            return;
        }
        btn_RoomCreate.interactable = false;

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        
        // 로비에서 RoomInfo에서 Room 커스텀프로퍼티 접근을 위함
        options.CustomRoomPropertiesForLobby = new string[] { "Map" };

        PhotonNetwork.CreateRoom(tmp_RoomName.text, options);

        UIManager.Instance.ClosePanel(gameObject);
        tmp_RoomName.text = null;
        btn_RoomCreate.interactable = true;
    }
}
