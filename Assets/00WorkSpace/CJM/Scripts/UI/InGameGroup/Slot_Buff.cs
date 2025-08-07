using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Buff : MonoBehaviour
{
    [SerializeField] Image image_Buff;
    [SerializeField] TMP_Text tmp_Duration;
    public float startTime;
    public float duration;

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
            tmp_Duration.transform.parent.gameObject.SetActive(false);

            image_Buff.sprite = sprite;
            this.duration = duration;
        }
        else
        {
            gameObject.SetActive(true);
            tmp_Duration.transform.parent.gameObject.SetActive(true);

            image_Buff.sprite = sprite;
            this.duration = duration;

            startTime = Time.time;
        }
    }

    public void UpdateView()
    {
        if (duration <= -98)
        {

        }
        else
        {
            duration -= Time.deltaTime;

            // ���� ���ӽð� ���� ��
            if (duration < 0)
            {
                // �� ������ ������ �̵�
                if (transform.parent.gameObject.activeSelf)
                    transform.SetAsLastSibling();

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
