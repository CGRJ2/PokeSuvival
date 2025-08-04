using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_InGameServerList : MonoBehaviour
{
    [SerializeField] Transform serverSlotsParent;
    Slot_InGameServer[] slots;
    public Panel_ServerInfo panel_ServerInfo;
    [SerializeField] Button btn_Esc;
    [SerializeField] Button btn_Refresh;
    [SerializeField] GameObject panel_Loading;

    bool isInited = false;

    public void Init()
    {
        slots = serverSlotsParent.GetComponentsInChildren<Slot_InGameServer>();
        panel_ServerInfo.Init();
        Debug.Log($"슬롯 개수:{slots.Length}");
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
        btn_Refresh.onClick.AddListener(UpdateServerListView);
    }

    public void SetDefaultSettings()
    {
        gameObject.SetActive(false);
        panel_ServerInfo.gameObject.SetActive(false);
    }

    private void OnDestroy() => StopAllCoroutines();

    private void OnEnable()
    {
        if (!isInited)
        {
            InitUpdateServerListView();
            isInited = true;
        }

        UpdateServerListView();
    }

    private void InitUpdateServerListView()
    {
        List<ServerData> serverList = new List<ServerData>();

        BackendManager.Instance.LoadAllTargetTypeServers(ServerType.InGame, (serverDic) =>
        {
            foreach (var kvp in serverDic)
            {
                serverList.Add(kvp.Value);
            }

            //Debug.Log($"인게임서버 개수: {serverList.Count}");

            for (int i = 0; i < slots.Length; i++)
            {
                // 불러올 방 정보가 더 이상 없다면
                if (serverList.Count - 1 < i)
                {
                    slots[i].HideSlotView();
                }
                else
                {
                    slots[i].InitSlotView(serverList[i]);
                }
            }
        });
    }


    public void UpdateServerListView()
    {
        panel_Loading.SetActive(true);


        List<ServerData> serverList = new List<ServerData>();

        BackendManager.Instance.LoadAllTargetTypeServers(ServerType.InGame, (serverDic) =>
        {
            foreach (var kvp in serverDic)
            {
                serverList.Add(kvp.Value);
            }

            //Debug.Log($"인게임서버 개수: {serverList.Count}");

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

            panel_Loading.SetActive(false);
        });
    }
}
