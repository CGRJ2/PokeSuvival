using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class Panel_Option : MonoBehaviour
{
    [SerializeField] Button btn_Esc;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider slider_Master;
    [SerializeField] Slider slider_FX;
    [SerializeField] Slider slider_BGM;
    
    public void Init()
    {
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));

        // �ʱ�ȭ: �����̴��� ���� dB ���� �ݿ� (optional)
        //SetSliderFromMixer(slider_Master, "Master");
        //SetSliderFromMixer(slider_BGM, "BGM");
        //SetSliderFromMixer(slider_FX, "Fx");

        // �߰������� ��� ���߱�
        slider_Master.value = 0.5f;
        slider_BGM.value = 0.5f;
        slider_FX.value = 0.5f;

        // �����̴� �̺�Ʈ ����
        slider_Master.onValueChanged.AddListener((value) => SetVolume("Master", value));
        slider_BGM.onValueChanged.AddListener((value) => SetVolume("BGM", value));
        slider_FX.onValueChanged.AddListener((value) => SetVolume("Fx", value));
    }


    void SetVolume(string parameterName, float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }

    void SetSliderFromMixer(Slider slider, string parameterName)
    {
        if (audioMixer.GetFloat(parameterName, out float currentDb))
        {
            slider.value = Mathf.Pow(10f, currentDb / 20f); // dB �� Linear(0~1)
        }
    }
}
