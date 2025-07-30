using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_GameOver : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_EXP;
    [SerializeField] TMP_Text tmp_Level;
    [SerializeField] TMP_Text tmp_Kills;
    [SerializeField] Button btn_Restart;
    [SerializeField] Button btn_ChangePok;
    [SerializeField] Button btn_Lobby;

    public void Init()
    {
        btn_Restart.onClick.AddListener(Restart);
        btn_ChangePok.onClick.AddListener(OpenSelectStartingPanel);
        btn_Lobby.onClick.AddListener(ReturnToLobbyServer);
    }

    private void Restart()
    {
        UIManager.Instance.InGameGroup.GameStartViewUpdate();

        // 여기서 플레이어 재시작 함수 실행
        PlayerManager pm = PlayerManager.Instance;
        if (pm != null)
        {
            pm.PlayerRespawn();
        }
    }

    private void OpenSelectStartingPanel()
    {
        UIManager um = UIManager.Instance;
        um.OpenPanel(um.StaticGroup.panel_SelectStarting.gameObject);
    }

    private void ReturnToLobbyServer()
    {
        NetworkManager.Instance.MoveToLobby();
    }

    // 플레이어 사망 시 호출
    public void UpdateResultView(float exp, int level, int kills)
    {
        tmp_EXP.text = $"EXP: {exp}";
        tmp_Level.text = $"Level: {level}";
        tmp_Kills.text = $"Kills: {kills}";
    }
}
