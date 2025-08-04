using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
// 이 컴포넌트를 사용하는 gameObject에 AudioSource 컴포넌트가 반드시 포함되도록 강제함
public class SoundPlayer : MonoBehaviour
{
    public AudioClip clip;// 재생할 오디오 클립
    public AudioMixerGroup mixerGroup;// 사용할 오디오 믹서 그룹 예: SFX, UI, BGM 등
    public bool playOnAwake = false;// true면 오브젝트 생성 시 자동 재생
    public bool is3D = false;// true면 3D 사운드거리 기반, false면 2D 사운드

    private AudioSource audioSource;// 오디오 재생용 AudioSource 참조

    void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 가져오기
        audioSource.outputAudioMixerGroup = mixerGroup;// 오디오 믹서 그룹 설정 BGM/SFX/UI 등 분리 재생 조절 가능
        audioSource.playOnAwake = playOnAwake;// 시작 시 자동 재생 여부 설정
        audioSource.spatialBlend = is3D ? 1f : 0f;// 3D/2D 사운드 설정,0 = 완전한 2D 사운드,1 = 완전한 3D 사운드
    }

    public void Play() // 외부에서 수동으로 호출하여 효과음 재생
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);// 지정된 오디오 클립을 OneShot으로 재생
            //OneShot은 중복 재생 가능하며 clip 설정을 바꾸지 않음
        }
    }
}