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
                // �� ������ �� Ŭ�� ���� ���ġ ����
                if (activedPlayerList[i - 1].Model.TotalExp < activedPlayerList[i].Model.TotalExp)
                {
                    SortPlayerListOrederByScore(activedPlayerList[i]);
                    return; // ���ġ �� �ٽ� ó������ ��ȸ
                }
            }
        }
    }

    private void SortPlayerListOrederByScore(PlayerController player)
    {
        activedPlayerList.Remove(player);

        // ��Ȯ�� ��ġ���� ã��
        for (int i = 0; i < activedPlayerList.Count; i++)
        {
            if (player.Model.TotalExp >= activedPlayerList[i].Model.TotalExp)
            {
                activedPlayerList.Insert(i, player);
                return;
            }
        }

        // ���� ���� ������ ��� �� ���� �߰�
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
