using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_RoomInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_RoomName;
    [SerializeField] TMP_Text tmp_PlayerCount;
    [SerializeField] Button btn_Enter;
    [SerializeField] Button btn_Esc;

    public void Init()
    {
        btn_Enter.onClick.AddListener(EnterRoom);
    }

    public void SetDefaultSetting()
    {
        tmp_RoomName.text = "";
        tmp_PlayerCount.text = "";
        gameObject.SetActive(false);
    }

    public void UpdatePanelView(RoomInfo roomInfo)
    {
        tmp_RoomName.text = roomInfo.Name;
        tmp_PlayerCount.text = $"{roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
    }

    public void EnterRoom()
    {
        PhotonNetwork.JoinRoom(tmp_RoomName.text);     // 방 입장 요청
        UIManager.Instance.ClosePanel(gameObject);
    }
}
