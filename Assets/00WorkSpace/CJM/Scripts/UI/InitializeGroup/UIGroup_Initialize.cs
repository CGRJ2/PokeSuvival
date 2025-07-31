using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_Initialize : MonoBehaviour
{
    public Panel_InitDefault panel_InitDefault;
    public Panel_PlayerInit panel_PlayerInit;
    public Panel_LogIn panel_LogIn;
    public Panel_SignUp panel_SignUp;

    public void Init()
    {
        panel_InitDefault.Init();
        panel_PlayerInit.Init();
        panel_LogIn.Init();
        panel_SignUp.Init();
    }

    public void InitView()
    {
        gameObject.SetActive(true);
        panel_InitDefault.gameObject.SetActive(true);
    }
}
