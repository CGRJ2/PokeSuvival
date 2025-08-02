using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Panel_MapSettings : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_MapName;
    [SerializeField] Image image_Map;
    [SerializeField] Button btn_ChangeMap;

    string selectedMapName;

    public void Init()
    {
        btn_ChangeMap.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_InGameServerList.gameObject));
    }


    public void MasterClientViewUpdate(bool isMaster)
    {
        if (isMaster)
        {
            btn_ChangeMap.interactable = true;
            btn_ChangeMap.gameObject.SetActive(true);
        }
        else
        {
            btn_ChangeMap.interactable = false;
            btn_ChangeMap.gameObject.SetActive(false);
        }
    }

    public void InitRoomSettings(bool isMaster)
    {
        // 처음 입장한 사람이라면 Ready상태 false
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["Ready"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        MasterClientViewUpdate(isMaster);
    }

    public void ChangeMap(string mapName)
    {
        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
        roomProperty["Map"] = mapName;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
    }

    public void UpdateRoomProperty()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties["Map"] != null)
        {
            selectedMapName = (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
            SpritesDB spritesDB = Resources.Load<SpritesDB>("SpriteDicSO/SpritesDB");
            image_Map.sprite = spritesDB.dic[selectedMapName];
            tmp_MapName.text = selectedMapName;
        }
        else
        {
            BackendManager.Instance.LoadAllTargetTypeServers(ServerType.InGame, (data) =>
            {
                foreach (var kvp in data)
                {
                    // 데이터가 있는 맨앞 녀석만 가져가기
                    if (!string.IsNullOrEmpty(kvp.Value.name))
                    {
                        selectedMapName = kvp.Value.name;
                        break;
                    }
                }

                ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
                roomProperty["Map"] = selectedMapName;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);

                SpritesDB spritesDB = Resources.Load<SpritesDB>("SpriteDicSO/SpritesDB");
                image_Map.sprite = spritesDB.dic[selectedMapName];
                tmp_MapName.text = selectedMapName;
            });


            
        }
        
        
    }

}
