using Photon.Pun;
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
        // 랭킹 보드 1~10위 업데이트
        BackendManager.Instance.UpdateRankingBoard_SortedByScore((kvpList) =>
        {
            for (int i = 0; i < slot_RankDatas.Length; i++)
            {
                if (i < kvpList.Count)
                {
                    string rankAndName = $"{i + 1}. {kvpList[i].Value.userName}";
                    string score = $"{kvpList[i].Value.highScore}";
                    slot_RankDatas[i].UpdateView(rankAndName, score);
                }
                else
                {
                    slot_RankDatas[i].UpdateView($"{i + 1}. 순위 데이터 없음", "-");
                }
            }
        });


        // 내 랭킹 표시
        string userId = "";
        if (BackendManager.Auth.CurrentUser != null)
            userId = $"{BackendManager.Auth.CurrentUser.UserId}";
        else
            userId = $"Guest({PhotonNetwork.LocalPlayer.UserId})";

        BackendManager.Instance.GetRankNumb(userId, (rankNumb) =>
            {
                BackendManager.Instance.LoadLocalPlayerRankData(userId, (rankData) =>
                {
                    string rankAndName = $"{rankNumb + 1}. {rankData.userName}";
                    string score = $"{rankData.highScore}";
                    slot_RankMine.UpdateView(rankAndName, score);
                },
                (failMsg) =>
                {
                    Debug.Log(failMsg);
                    slot_RankMine.UpdateView("순위 데이터 없음", "-");
                });
            },
            (failMsg) =>
            {
                Debug.Log(failMsg);
                slot_RankMine.UpdateView("순위 데이터 없음", "-");
            });
    }
}
