using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup_InGame : MonoBehaviour
{
    public Panel_HUD panel_HUD;
    [SerializeField] private Panel_GameOver panel_GameOver;
    public Panel_GameOverAutoReturnLobby panel_GameOverAutoReturnLobby;

    public List<PlayerController> activedPlayerList = new List<PlayerController>();


    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach(PlayerController pc in activedPlayerList)
            {
                Debug.Log($"{pc.Model.PokeData.PokeName}의 딕셔너리: {pc.Rank.RankUpdic}");
            }
        }
    }*/

    public void Init()
    {
        panel_HUD.Init();
        panel_GameOver.Init();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // 인게임 시작 시 호출
    public void GameStartViewUpdate()
    {
        panel_HUD.gameObject.SetActive(true);
        panel_GameOver.gameObject.SetActive(false);

        panel_HUD.OnGameStart();
    }

    // 플레이어 사망 시 호출
    public void GameOverViewUpdate(PlayerController pc)
    {
        panel_HUD.gameObject.SetActive(false);

        StartCoroutine(WaitAndOpen());
        panel_GameOver.UpdateResultView(pc.Model.TotalExp, pc.Model.PokeLevel, pc.KillCount, pc.SurvivalTime);
        panel_HUD.panel_BuffState.InitSlots();
    }

    IEnumerator WaitAndOpen()
    {
        yield return new WaitForSeconds(2f);
        Debug.LogWarning("천천히 오픈");
        panel_GameOver.gameObject.SetActive(true);
    }

    public void UpdateSkillSlots(PlayerModel playerModel)
    {
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
