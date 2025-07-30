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

    // �ΰ��� ���� �� ȣ��
    public void GameStartViewUpdate()
    {
        panel_HUD.gameObject.SetActive(true);
        panel_GameOver.gameObject.SetActive(false);

        panel_HUD.OnGameStart();

        PlayerManager pm = PlayerManager.Instance;
        if (pm != null)
        {
            pm.PlayerInstaniate();
        }

    }

    // �÷��̾� ��� �� ȣ��
    public void GameOverViewUpdate(PlayerModel playerModel)
    {
        // �̷��� ����Ͻø� �˴ϴ�
        //UIManager.Instance.InGameGroup.GameOverViewUpdate(playerModel);


        panel_HUD.gameObject.SetActive(false);
        panel_GameOver.gameObject.SetActive(true);
        panel_GameOver.UpdateResultView(11, 11, 11);
    }
}
