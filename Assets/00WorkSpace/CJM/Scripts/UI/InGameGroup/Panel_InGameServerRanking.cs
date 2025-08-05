using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Panel_InGameServerRanking : MonoBehaviour
{
    Slot_RankData[] slots;
    [SerializeField] Transform slotParent;

    public List<PlayerController> activedPlayerList = new List<PlayerController>();

    public void Init()
    {
        slots = slotParent.GetComponentsInChildren<Slot_RankData>();
    }

    private void Update()
    {
        CheckRankChange();
        UpdateRankingView();
    }

    private void CheckRankChange()
    {
        for (int i = 0; i < activedPlayerList.Count; i++)
        {
            if (i > 0)
            {
                // 뒷 순서가 더 클때 순위 재배치 실행
                if (activedPlayerList[i - 1].Model.TotalExp < activedPlayerList[i].Model.TotalExp)
                {
                    SortPlayerListOrederByScore(activedPlayerList[i]);
                    return; // 재배치 후 다시 처음부터 순회
                }
            }
        }
    }

    private void SortPlayerListOrederByScore(PlayerController player)
    {
        activedPlayerList.Remove(player);

        // 정확한 위치까지 찾기
        for (int i = 0; i < activedPlayerList.Count; i++)
        {
            if (player.Model.TotalExp >= activedPlayerList[i].Model.TotalExp)
            {
                activedPlayerList.Insert(i, player);
                return;
            }
        }

        // 가장 낮은 점수일 경우 맨 끝에 추가
        activedPlayerList.Add(player);
    }

    private void UpdateRankingView()
    {
        for(int i =0; i < slots.Length; i++)
        {
            if (i < activedPlayerList.Count)
            {
                string rankAndName = $"{i + 1}. {activedPlayerList[i].Model.PlayerName}";
                string score = $"{activedPlayerList[i].Model.TotalExp}";
                slots[i].UpdateView(rankAndName, score);
            }
            else
            {
                slots[i].UpdateView($"{i + 1}. -", "-");
            }
        }
    }
}
