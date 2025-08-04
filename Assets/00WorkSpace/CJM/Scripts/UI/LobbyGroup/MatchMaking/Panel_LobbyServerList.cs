using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyServerList : MonoBehaviour
{
    [SerializeField] Transform serverSlotsParent;
    Slot_ServerInfo_Lobby[] slots;
    public Panel_ServerInfo panel_ServerInfo;
    [SerializeField] Button btn_Esc;
    [SerializeField] Button btn_Refresh;

    public void Init()
    {
        slots = serverSlotsParent.GetComponentsInChildren<Slot_ServerInfo_Lobby>();
        panel_ServerInfo.Init();

        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
        btn_Refresh.onClick.AddListener(UpdateServerListView);
    }

    public void SetDefaultSetting()
    {
        panel_ServerInfo.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateServerListView();
    }

    public void UpdateServerListView()
    {
        List<ServerData> serverList = new List<ServerData>();

        BackendManager.Instance.LoadAllTargetTypeServers(ServerType.Lobby, (serverDic) =>
        {
            foreach (var kvp in serverDic)
            {
                serverList.Add(kvp.Value);
            }


            for (int i = 0; i < slots.Length; i++)
            {
                // 불러올 방 정보가 더 이상 없다면
                if (serverList.Count - 1 < i)
                {
                    slots[i].HideSlotView();
                }
                else
                {
                    slots[i].UpdateSlotView(serverList[i]);
                }
            }
        });
    }
}
