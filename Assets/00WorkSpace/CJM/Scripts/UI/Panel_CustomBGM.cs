using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_CustomBGM : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_CurBGM;
    public AudioSource audioSource;
    [SerializeField] Transform slotsParent;
    Slot_BGM[] slots;
    [SerializeField] Button btn_ESC;

    [SerializeField] AudioClip[] audioClips;

    void Update()
    {
        tmp_CurBGM.text = $"ÇöÀç BGM: {audioSource.clip?.name}";
        if (audioSource.clip == null) tmp_CurBGM.text = "BGM:";
    }

    public void Init()
    {
        btn_ESC.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));

        slots = slotsParent.GetComponentsInChildren<Slot_BGM>();

        for (int i =0; i < slots.Length; i++)
        {
            slots[i].InitSlot(audioClips[i]);
        }
    }

    public void SetNewAudioClipAndPlay(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void RandomAudioClipPlay()
    {
        int r = Random.Range(0, audioClips.Length - 1);
        audioSource.clip = audioClips[r];
        audioSource.Play();
    }

    public bool IsBGMNullOrInitial()
    {
        if (audioSource == null || audioSource == UIManager.Instance.InitializeGroup.InitializeDefaultBGM)
            return true;
        else return false;
    }
}
