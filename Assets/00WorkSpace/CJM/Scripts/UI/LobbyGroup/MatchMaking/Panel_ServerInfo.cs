using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        // 로비 서버의 전환이라면. 
        switch (selectedServerData.type)
        {
            case (int)ServerType.Lobby:
                // 그냥 서버만 전환해주면 됨. 서버 리스트 업데이트 해주고
                //if (SceneManager.GetActiveScene().name == )
                break;

            case (int)ServerType.InGame:

                break;

            default:
                break;
        }



        UIManager.Instance.ClosePanel(gameObject);
    }
}
