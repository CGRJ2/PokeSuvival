using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_GuestInit : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField_Name;
    [SerializeField] Button btn_Create;

    public void Init()
    {
        btn_Create.onClick.AddListener(NicknameAdmit);
    }


    public void NicknameAdmit()
    {
        if (string.IsNullOrWhiteSpace(inputField_Name.text))
        {
            Debug.LogError("�г����� ����� �������ּ���");
            return;
        }

        PhotonNetwork.NickName = inputField_Name.text;
        PhotonNetwork.JoinLobby();
        gameObject.SetActive(false);
        
    }
}
