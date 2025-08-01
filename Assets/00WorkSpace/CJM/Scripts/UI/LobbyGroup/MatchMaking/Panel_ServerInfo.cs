using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_ServerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_ServerName;
    [SerializeField] Button btn_Confirm;
    [SerializeField] Button btn_Cancel;

    public ServerData selectedServerData;

    public void Init()
    {
        btn_Confirm.onClick.AddListener(MoveToTargetServer);
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    public void UpdateServerDataAndView(ServerData serverData)
    {
        selectedServerData = serverData;
        tmp_ServerName.text = selectedServerData.name;
    }

    private void MoveToTargetServer()
    {
        NetworkManager.Instance.ChangeServer(selectedServerData);

        UIManager.Instance.ClosePanel(gameObject);
    }
}
