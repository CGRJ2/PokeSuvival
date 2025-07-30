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

        PlayerManager pm = PlayerManager.Instance;
        if (pm != null)
        {
            pm.PlayerInstaniate();
        }

    }

    // 플레이어 사망 시 호출
    public void GameOverViewUpdate(PlayerModel playerModel)
    {
        // 이렇게 사용하시면 됩니다
        //UIManager.Instance.InGameGroup.GameOverViewUpdate(playerModel);


        panel_HUD.gameObject.SetActive(false);
        panel_GameOver.gameObject.SetActive(true);
        panel_GameOver.UpdateResultView(11, 11, 11);
    }
}
