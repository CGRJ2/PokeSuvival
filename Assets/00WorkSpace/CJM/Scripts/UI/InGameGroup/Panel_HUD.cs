using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_HUD : MonoBehaviour
{
    public Panel_PlayerStatus panel_PlayerStatus;

    public void Init()
    {
        panel_PlayerStatus.Init();
    }

    public void OnGameStart()
    {

    }
}
