using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_InitDefault : MonoBehaviour
{
    [SerializeField] Button btn_LogIn;
    [SerializeField] Button btn_CreateAccount;
    [SerializeField] Button btn_GuestStart;

    public void Init()
    {
        UIManager um = UIManager.Instance;
        btn_LogIn.onClick.AddListener(() => um.OpenPanel(um.InitializeGroup.panel_LogIn.gameObject));
        btn_CreateAccount.onClick.AddListener(() => um.OpenPanel(um.InitializeGroup.panel_SignUp.gameObject));
        btn_GuestStart.onClick.AddListener(() => um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject));
    }
}
