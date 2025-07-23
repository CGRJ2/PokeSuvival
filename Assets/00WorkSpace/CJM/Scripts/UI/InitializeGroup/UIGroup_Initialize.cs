using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_Initialize : MonoBehaviour
{
    public Panel_InitDefault panel_InitDefault;
    public Panel_GuestInit panel_GuestInit;


    public void Init()
    {
        panel_InitDefault.Init();
        panel_GuestInit.Init();
    }

    public void InitView()
    {
        gameObject.SetActive(true);
        panel_InitDefault.gameObject.SetActive(true);
    }
}
