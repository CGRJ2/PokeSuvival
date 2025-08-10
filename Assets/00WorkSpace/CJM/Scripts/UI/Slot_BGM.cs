using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_BGM : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] Button btn_Self;
    [SerializeField] AudioClip audioClip;


    public void InitSlot(AudioClip audioClip)
    {
        tmp_Name.text = audioClip.name;
        this.audioClip = audioClip;
        btn_Self.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        UIManager.Instance.StaticGroup.panel_CustomBGM.SetNewAudioClipAndPlay(audioClip);
    }
}
