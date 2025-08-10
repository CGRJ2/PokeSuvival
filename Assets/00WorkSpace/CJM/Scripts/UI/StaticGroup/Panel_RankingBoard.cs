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
        // ��ŷ ���� 1~10�� ������Ʈ
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
                    slot_RankDatas[i].UpdateView($"{i + 1}. ���� ������ ����", "-");
                }
            }
        });


        // �� ��ŷ ǥ��
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
                    slot_RankMine.UpdateView("���� ������ ����", "-");
                });
            },
            (failMsg) =>
            {
                Debug.Log(failMsg);
                slot_RankMine.UpdateView("���� ������ ����", "-");
            });
    }
}
