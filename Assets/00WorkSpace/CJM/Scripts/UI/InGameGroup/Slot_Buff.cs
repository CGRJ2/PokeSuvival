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
        // ���� ��Ȱ��ȭ ��, �� ������ ������ �̵�
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

        // ���� ���ӽð� ���� ��
        if (duration < 0)
        {
            gameObject.SetActive(false);
        }
        // ���� ���ӽð� ����
        else
        {
            tmp_Duration.text = $"{Mathf.Clamp((int)duration, 0, 999)}";
        }

    }
}
