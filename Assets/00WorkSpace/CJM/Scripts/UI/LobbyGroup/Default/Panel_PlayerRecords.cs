using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Firebase.Auth;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Panel_PlayerRecords : MonoBehaviour
{
    [SerializeField] Button btn_DropDown;
    [SerializeField] TMP_Text tmp_HighScore;
    [SerializeField] TMP_Text tmp_Kills;
    [SerializeField] TMP_Text tmp_SuvivalTime;
    [SerializeField] TMP_Text tmp_Money;

    RectTransform rect;
    [SerializeField] Vector2 closePos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float duration;
    public bool isOpened = false;

    public void Init()
    {
        rect = GetComponent<RectTransform>();
        btn_DropDown.onClick.AddListener(SwitchToggleDropDownButton);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void UpdateView()
    {
        StartCoroutine(WaitUntilUserInfoLoaded());
    }

    private IEnumerator WaitUntilUserInfoLoaded()
    {
        // 이런 WaitUntil로 무한 대기하는 구조들을 콜백 기반으로 리팩토링해주는 작업 필요 (TODO)
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Money"));
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("HighScore"));
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Kills"));
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("SuvivalTime"));

        int money = (int)PhotonNetwork.LocalPlayer.CustomProperties["Money"];
        int kills = (int)PhotonNetwork.LocalPlayer.CustomProperties["Kills"];
        float highScore = (float)PhotonNetwork.LocalPlayer.CustomProperties["HighScore"];
        float suvivalTime = (float)PhotonNetwork.LocalPlayer.CustomProperties["SuvivalTime"];
        int minutes = Mathf.FloorToInt(suvivalTime / 60f);
        int seconds = Mathf.FloorToInt(suvivalTime % 60f);

        //Debug.Log($"기록 불러와졌나? {money}, {highScore}, {kills}, {suvivalTime}");

        tmp_HighScore.text = $"최고 점수: {highScore}";
        tmp_Kills.text = $"최대 킬 수:{kills}";
        tmp_SuvivalTime.text = string.Format("최대 생존 시간: {0:00}:{1:00}", minutes, seconds);

        tmp_Money.text = $"{money} G";
    }

    public void SwitchToggleDropDownButton()
    {

        if (isOpened)
        {
            rect.DOAnchorPos(closePos, duration).SetEase(Ease.OutCubic);
            isOpened = false;
        }
        else
        {
            rect.DOAnchorPos(openPos, duration).SetEase(Ease.OutCubic);
            isOpened = true;
        }
    }
}
