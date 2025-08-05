using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_HUD : MonoBehaviour
{
    public Panel_PlayerStatus panel_PlayerStatus;
    public Panel_SkillSlots panel_SkillSlots;
    public Panel_UpperMenu panel_UpperMenu;
    public Panel_InGameServerRanking panel_InGameServerRanking;

    public void Init()
    {
        panel_PlayerStatus.Init();
        panel_SkillSlots.Init();
        panel_UpperMenu.Init();
        panel_InGameServerRanking.Init();
    }

    public void OnGameStart()
    {

    }
}
