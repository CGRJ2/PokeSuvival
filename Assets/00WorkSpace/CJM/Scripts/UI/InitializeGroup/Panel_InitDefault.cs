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
        btn_LogIn.onClick.AddListener(OpenLogInPanel);
        btn_CreateAccount.onClick.AddListener(OpenSignUpPanel);
        btn_GuestStart.onClick.AddListener(OpenPlayerInitPanel);
    }

    public void OpenPlayerInitPanel()
    {
        UIManager.Instance.InitializeGroup.panel_PlayerInit.gameObject.SetActive(true);
    }

    public void OpenLogInPanel()
    {
        UIManager.Instance.InitializeGroup.panel_LogIn.gameObject.SetActive(true);
    }

    public void OpenSignUpPanel()
    {
        UIManager.Instance.InitializeGroup.panel_SignUp.gameObject.SetActive(true);
    }
}
