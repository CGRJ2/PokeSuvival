using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_InGameServer : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_ServerName;
    [SerializeField] TMP_Text tmp_PlayerCount;
    [SerializeField] Button btn_Self;
    [SerializeField] Image image_RegionIllustration;
    [SerializeField] GameObject image_CurServerOutline;
    public ServerData serverData;

    public void InitSlotView(ServerData serverData)
    {
        btn_Self.onClick.AddListener(SetSelectedServerData);

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        this.serverData = serverData;
        tmp_ServerName.text = serverData.name;

        SpritesDB spritesDB = Resources.Load<SpritesDB>("SpriteDicSO/SpritesDB");
        image_RegionIllustration.sprite = spritesDB.dic[serverData.name];
    }

    public void UpdateSlotView(ServerData serverData)
    {
        Debug.Log("개별 슬롯 업데이트 실행");
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        
        tmp_PlayerCount.text = $"{serverData.curPlayerCount} / {serverData.maxPlayerCount}";

        if (serverData.name == NetworkManager.Instance.CurServer.name)
        {
            image_CurServerOutline.SetActive(true);
        }
        else
        {
            image_CurServerOutline.SetActive(false);
        }
    }

    public void HideSlotView()
    {
        gameObject.SetActive(false);
    }
    private void SetSelectedServerData()
    {
        if (serverData.name == NetworkManager.Instance.CurServer.name) { Debug.Log("이미 해당 서버에 존재합니다."); return; }

        if (serverData != null)
        {
            UIManager um = UIManager.Instance;
            // 로비 서버에서 && 파티(방)에서 선택창을 열었다면 => 룸 프로퍼티 설정(ChangeMap)
            if (NetworkManager.Instance.CurServer.type == (int)ServerType.Lobby
                && PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Joined)
            {
                um.LobbyGroup.panel_RoomInside.panel_MapSettings.ChangeMap(serverData.key);
                um.ClosePanel(um.StaticGroup.panel_InGameServerList.gameObject);
            }

            else
            {
                um.OpenPanel(um.StaticGroup.panel_InGameServerList.panel_ServerInfo.gameObject);
                um.StaticGroup.panel_InGameServerList.panel_ServerInfo.UpdateServerDataAndView(serverData);
            }
        }
        else
        {
            Debug.LogError("선택한 슬롯에 서버 정보가 없음");
        }
    }
}
