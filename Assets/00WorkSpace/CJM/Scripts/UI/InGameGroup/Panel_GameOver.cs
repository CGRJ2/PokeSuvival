using Photon.Pun;
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
        gameObject.SetActive(false); // SJH
        NetworkManager.Instance.MoveToLobby();
    }

    // 플레이어 사망 시 호출
    public void UpdateResultView(float score, int level, int kills, float suvivalTime)
    {
        tmp_EXP.text = $"EXP: {score}";
        tmp_Level.text = $"Level: {level}";
        tmp_Kills.text = $"Kills: {kills}";

        // 돈 계산 수식 여기에 업데이트 (현재는 임시로 넣어두었습니다)
        int rewardMoneyValue = (int)(score / 10) + level + kills;

        // 유저 데이터 업데이트
        // 유저 데이터 있음(로그인 유저)
        if (BackendManager.Auth.CurrentUser != null)
        {
            string userId = $"{BackendManager.Auth.CurrentUser.UserId}";

            BackendManager.Instance.LoadUserDataFromDB((userData) =>
            {
                UserData updatedUserData = userData;

                updatedUserData.money += rewardMoneyValue;

                if (score > updatedUserData.highScore)
                    updatedUserData.highScore = score;

                if (suvivalTime > updatedUserData.suvivalTime)
                    updatedUserData.suvivalTime = suvivalTime;

                if (kills > updatedUserData.kills)
                    updatedUserData.kills = kills;

                // 서버에 유저 데이터를 갱신
                BackendManager.Instance.InitUserDataToDB(updatedUserData, () =>
                {
                    // 갱신 완료 시 서버의 유저데이터를 클라이언트에 동기화
                    NetworkManager.Instance.UpdateUserDataToClient(userData);
                });
            });

            // 랭킹 데이터 체크 & 업데이트
            BackendManager.Instance.LoadLocalPlayerRankData(userId, (rankData) =>
            {
                if (rankData.highScore < score)
                {
                    BackendManager.Instance.UpdateHighScore((int)score, userId);
                }
            },
            (onFailMsg) => // 랭킹 데이터가 존재하지 않으면 생성해주기
            {
                BackendManager.Instance.InitLocalPlayerRankingData(new RankData(userId, PhotonNetwork.NickName, (int)score));
            });
        }
        // 유저 데이터 없음(게스트 유저)
        else
        {
            string userId = $"Guest({PhotonNetwork.LocalPlayer.UserId})";

            PhotonNetwork.LocalPlayer.CustomProperties["Money"] = rewardMoneyValue + (int)PhotonNetwork.LocalPlayer.CustomProperties["Money"];

            if (kills > (int)PhotonNetwork.LocalPlayer.CustomProperties["Kills"])
                PhotonNetwork.LocalPlayer.CustomProperties["Kills"] = kills;

            if (score > (float)PhotonNetwork.LocalPlayer.CustomProperties["HighScore"])
                PhotonNetwork.LocalPlayer.CustomProperties["HighScore"] = score;

            if (suvivalTime > (float)PhotonNetwork.LocalPlayer.CustomProperties["SuvivalTime"])
                PhotonNetwork.LocalPlayer.CustomProperties["SuvivalTime"] = suvivalTime;


            // 랭킹 데이터 체크 & 업데이트
            BackendManager.Instance.LoadLocalPlayerRankData(userId, (rankData) =>
            {
                if (rankData.highScore < score)
                {
                    BackendManager.Instance.UpdateHighScore((int)score, userId);
                }
            },
            (onFailMsg) => // 랭킹 데이터가 존재하지 않으면 생성해주기
            {
                BackendManager.Instance.InitLocalPlayerRankingData(new RankData(userId, PhotonNetwork.NickName, (int)score));
            });
        }


        

    }
}
