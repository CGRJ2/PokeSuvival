using TMPro;
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
        if (duration <= -98)
        {
            gameObject.SetActive(true);
            Debug.LogWarning("�нú� ���� ����");
            image_Buff.sprite = sprite;
            this.duration = duration;
            tmp_Duration.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("��Ÿ�� ���� ����");

            gameObject.SetActive(true);
            tmp_Duration.transform.parent.gameObject.SetActive(true);

            image_Buff.sprite = sprite;

            startTime = Time.time;
            this.duration = duration;
        }
    }

    public void UpdateView()
    {
        if (duration <= -98)
        {
            Debug.LogWarning("�нú�� �ð� ���� ����");
        }
        else
        {
            Debug.LogWarning("���ӽð� ��� ����");

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
}
