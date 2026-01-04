using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Panel_RankingBoard : MonoBehaviour
{
    [SerializeField] Transform slotsParent;
    Slot_RankData[] slot_RankDatas;
    [SerializeField] Slot_RankData slot_RankMine;

    [SerializeField] Button btn_Esc;


    public void Init()
    {
        slot_RankDatas = slotsParent.GetComponentsInChildren<Slot_RankData>();

        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void OnEnable()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        string userId = NetworkManager.Instance.GetUserId();

        // 모든 랭크 데이터 불러오기
        BackendManager.Instance.LoadAllRankData(dic =>
        {
            // 1) 정렬
            var sorted = dic
                .OrderByDescending(kvp => kvp.Value.highScore)
                .ToList();

            // 2) 1 ~ 10위 정렬
            var top10 = sorted.Take(10).ToList();

            // 3) Top10 UI 업데이트
            for (int i = 0; i < slot_RankDatas.Length; i++)
            {
                if (i < top10.Count)
                {
                    string rankAndName = $"{i + 1}. {top10[i].Value.userName}";
                    string score = $"{top10[i].Value.highScore}";
                    slot_RankDatas[i].UpdateView(rankAndName, score);
                }
                else
                {
                    slot_RankDatas[i].UpdateView($"{i + 1}. 순위 데이터 없음", "-");
                }
            }

            // 4) 내 랭킹 찾기
            int myIndex = sorted.FindIndex(x => x.Value.userId == userId);
            if (myIndex >= 0)
            {
                var myData = sorted[myIndex].Value;
                slot_RankMine.UpdateView($"{myIndex + 1}. {myData.userName}", $"{myData.highScore}");
            }
            else
            {
                slot_RankMine.UpdateView("순위 데이터 없음", "-");
            }

        },
        failMsg =>
        {
            Debug.Log(failMsg);
            // 실패 시 UI 처리
            for (int i = 0; i < slot_RankDatas.Length; i++)
                slot_RankDatas[i].UpdateView($"{i + 1}. 순위 데이터 없음", "-");

            slot_RankMine.UpdateView("순위 데이터 없음", "-");
        });
    }
}
