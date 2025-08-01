using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_ServerInfo_Lobby : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_ServerName;
    [SerializeField] TMP_Text tmp_PlayerCount;
    [SerializeField] Button btn_Self;
    Image slotImage;
    ServerData serverData;

    private void Awake()
    {
        btn_Self.onClick.AddListener(SetSelectedServerData);
        slotImage = GetComponent<Image>();
    }

    private void SetSelectedServerData()
    {
        if (serverData == NetworkManager.Instance.CurServer) { Debug.Log("이미 해당 서버에 존재합니다."); return; }

        if (serverData != null)
        {
            UIManager.Instance.OpenPanel(UIManager.Instance.LobbyGroup.panel_MatchMaking.panel_LobbyServerList.panel_ServerInfo.gameObject);
            UIManager.Instance.LobbyGroup.panel_MatchMaking.panel_LobbyServerList.panel_ServerInfo.UpdateServerDataAndView(serverData);
        }
        else
        {
            Debug.LogError("선택한 슬롯에 서버 정보가 없음");
        }
    }

    public void UpdateSlotView(ServerData serverData)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        this.serverData = serverData;
        tmp_ServerName.text = serverData.name;
        tmp_PlayerCount.text = $"{serverData.curPlayerCount} / {serverData.maxPlayerCount}";

        if (serverData == NetworkManager.Instance.CurServer)
        {
            slotImage.color = Color.green;
        }
        else
        {
            slotImage.color = Color.white;
        }
    }

    public void HideSlotView()
    {
        gameObject.SetActive(false);
    }
}
