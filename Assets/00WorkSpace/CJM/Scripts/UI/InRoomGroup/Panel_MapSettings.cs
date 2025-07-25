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
    [SerializeField] Button btn_MapLeft;
    [SerializeField] Button btn_MapRight;
    

    int mapIndex;

    [Header("���߿� �����ͺ��̽� �и� �ʿ�")]
    [SerializeField] MapData[] mapDatas; // ���߿� �����ͺ��̽����� �ҷ����� ������� �����ؾߵ� ��


    public void Init()
    {
        btn_MapLeft.onClick.AddListener(ClickLeftMapButton);
        btn_MapRight.onClick.AddListener(ClickRightMapButton);
    }


    public void InitRoomSettings(bool isMaster)
    {
        // ó�� ������ ����̶�� Ready���� false
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["Ready"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

        // ���� or Not ����
        if (isMaster)
        {
            btn_MapLeft.interactable = true;
            btn_MapLeft.gameObject.SetActive(true);

            btn_MapRight.interactable = true;
            btn_MapRight.gameObject.SetActive(true);
        }
        else
        {
            btn_MapLeft.interactable = false;
            btn_MapLeft.gameObject.SetActive(false);

            btn_MapRight.interactable = false;
            btn_MapRight.gameObject.SetActive(false);
        }

    }

    public void ClickLeftMapButton()
    {
        if (mapIndex - 1 < 0) mapIndex = 0;
        else mapIndex--;

        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
        roomProperty["Map"] = mapIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);

        UpdateRoomProperty();
    }

    public void ClickRightMapButton()
    {
        if (mapIndex + 1 > mapDatas.Length - 1) mapIndex = mapDatas.Length - 1;
        else mapIndex++;

        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
        roomProperty["Map"] = mapIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);

        UpdateRoomProperty();
    }

    public void UpdateRoomProperty()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties["Map"] != null)
        {
            mapIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
        }
        image_Map.sprite = mapDatas[mapIndex].sprite;
        tmp_MapName.text = mapDatas[mapIndex].name;
    }

}
