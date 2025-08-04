using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_InGame : MonoBehaviour
{
    [SerializeField] private Panel_HUD panel_HUD;
    [SerializeField] private Panel_GameOver panel_GameOver;
    public Panel_ReturnToLobbyConfirm panel_ReturnToLobbyConfirm;



    public void Init()
    {
        panel_HUD.Init();
        panel_GameOver.Init();
        panel_ReturnToLobbyConfirm.Init();
    }

    // �ΰ��� ���� �� ȣ��
    public void GameStartViewUpdate()
    {
        panel_HUD.gameObject.SetActive(true);
        panel_GameOver.gameObject.SetActive(false);

        panel_HUD.OnGameStart();
    }

    // �÷��̾� ��� �� ȣ��
    public void GameOverViewUpdate(PlayerModel playerModel)
    {
        panel_HUD.gameObject.SetActive(false);
        panel_GameOver.gameObject.SetActive(true);

        // ų��&�����ð��� ��� �ӽ÷� �־��
        panel_GameOver.UpdateResultView(playerModel.TotalExp, playerModel.PokeLevel, 99, playerModel.EndTime - playerModel.StartTime);
    }

    public void UpdateSkillSlots(PlayerModel playerModel)
    { 
        // �̷��� ����ϸ� �˴ϴ�
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
