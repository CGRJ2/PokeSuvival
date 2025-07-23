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
        btn_GuestStart.onClick.AddListener(OpenGuestInitPanel);
    }

    public void OpenGuestInitPanel()
    {
        UIManager.Instance.InitializeGroup.panel_GuestInit.gameObject.SetActive(true);
    }
}
