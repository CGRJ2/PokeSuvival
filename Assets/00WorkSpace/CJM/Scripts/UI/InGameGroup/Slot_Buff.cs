using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Buff : MonoBehaviour
{
    [SerializeField] Image image_Buff;
    [SerializeField] TMP_Text tmp_Duration;
    float startTime;
    float duration;

    private void OnDisable()
    {
        // 슬롯 비활성화 시, 맨 마지막 순서로 이동
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            UpdateView();
        }
    }

    public void UpdateBuffSlot(Sprite sprite, float duration)
    {
        gameObject.SetActive(true);
        image_Buff.sprite = sprite;

        startTime = Time.time;
        this.duration = duration;
    }

    public void UpdateView()
    {
        float remainTime = duration - (Time.time - startTime);

        // 버프 지속시간 종료 시
        if (remainTime < 0)
        {
            gameObject.SetActive(false);
        }
        // 버프 지속시간 동안
        else
        {
            int minutes = Mathf.FloorToInt(remainTime / 60f);
            int seconds = Mathf.FloorToInt(remainTime % 60f);
            tmp_Duration.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

    }
}
