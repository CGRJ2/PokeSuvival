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
    public float startTime;
    public float duration;
    private void OnDisable()
    {
        // 슬롯 비활성화 시, 맨 마지막 순서로 이동
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (gameObject.activeSelf)
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
        duration -= Time.deltaTime;

        // 버프 지속시간 종료 시
        if (duration < 0)
        {
            gameObject.SetActive(false);
        }
        // 버프 지속시간 동안
        else
        {
            tmp_Duration.text = $"{Mathf.Clamp((int)duration, 0, 999)}";
        }

    }
}
