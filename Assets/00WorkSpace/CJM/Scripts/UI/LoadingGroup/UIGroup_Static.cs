using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_Static : MonoBehaviour
{
    public Panel_Loading panel_Loading;
    public Panel_SelectStarting panel_SelectStarting;
    public Panel_Option panel_Option;
    public Panel_InGameServerList panel_InGameServerList;
    //public LoadingPanel serverPanel;

    public void Init()
    {
        panel_SelectStarting.Init();
        panel_Option.Init();
        panel_InGameServerList.Init();
    }


    public void SetDefaultSettings()
    {
        panel_InGameServerList.SetDefaultSettings();
        UIManager.Instance.ClearPanelStack();
    }
    
}
