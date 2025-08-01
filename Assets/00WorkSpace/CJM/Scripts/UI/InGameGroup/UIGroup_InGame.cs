using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_InGame : MonoBehaviour
{
    [SerializeField] private Panel_HUD panel_HUD;
    [SerializeField] private Panel_GameOver panel_GameOver;


    public void Init()
    {
        panel_HUD.Init();
        panel_GameOver.Init();
    }

    // 인게임 시작 시 호출
    public void GameStartViewUpdate()
    {
        panel_HUD.gameObject.SetActive(true);
        panel_GameOver.gameObject.SetActive(false);

        panel_HUD.OnGameStart();
    }

    // 플레이어 사망 시 호출
    public void GameOverViewUpdate(PlayerModel playerModel)
    {
        panel_HUD.gameObject.SetActive(false);
        panel_GameOver.gameObject.SetActive(true);

        // 킬수는 잠시 임시로 넣어둠
        panel_GameOver.UpdateResultView(playerModel.TotalExp, playerModel.PokeLevel, 99);
    }

    public void UpdateSkillSlots(PlayerModel playerModel)
    { 
        // 이렇게 사용하면 됩니다
        // UIManager.Instance.InGameGroup.UpdateSkillSlots();
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            panel_HUD.panel_SkillSlots.UpdateSkillSlotsView(playerModel);
        }
    }

    public void UpdateCoolTime(PlayerModel playerModel, SkillSlot slot)
    {
        panel_HUD.panel_SkillSlots.UpdateSlotCoolTimeView(playerModel, slot);
    }
}
