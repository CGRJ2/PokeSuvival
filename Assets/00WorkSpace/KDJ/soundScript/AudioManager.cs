using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;// 싱글톤 인스턴스: 어디서든 접근 가능하게 하기 위해 static으로 선언

    [Header("믹서")]
    public AudioMixer mixer;// Unity에서 오디오 믹서를 사용해 소리의 볼륨 및 이펙트를 제어할 수 있음
    [Header("오디오 클립")]
    // 효과음과 배경음으로 사용할 오디오 클립들을 인스펙터에서 지정
    public AudioClip hitClip;// 피격 효과음
    public AudioClip itemClip;// 아이템 획득 효과음
    public AudioClip uiClip;// UI 클릭 효과음
    public AudioClip bgmClip;// 배경음악

    [Header("오디오 소스")]
    // 오디오 재생에 사용되는 오디오 소스들
    public AudioSource sfxSource;// 효과음용 AudioSource 2D
    public AudioSource bgmSource;// 배경음악용 AudioSource

    private void Awake()
    {
        // 싱글톤 초기화: 인스턴스가 없으면 이 객체를 할당, 있으면 중복 제거
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        PlayBGM(bgmClip);// 게임 시작 시 배경음 자동 재생
    }
    public void PlaySFX(AudioClip clip)// 효과음 재생 함수OneShot으로 중복 재생 가능
    {
        sfxSource.PlayOneShot(clip);
    }

    // 각각의 효과음을 편리하게 호출할 수 있는 함수들
    public void PlayHit() => PlaySFX(hitClip);// 피격 효과음 재생
    public void PlayItem() => PlaySFX(itemClip);// 아이템 획득 효과음 재생
    public void PlayUI() => PlaySFX(uiClip);// UI 클릭 효과음 재생

     public void PlayBGM(AudioClip clip)// 배경음 재생 함수 
    {
        bgmSource.clip = clip;// 재생할 배경음 지정
        bgmSource.loop = true;// 반복 재생 설정
        bgmSource.Play();// 배경음 재생 시작
    }
}