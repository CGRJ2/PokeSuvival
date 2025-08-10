using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SmartSoundTrigger : MonoBehaviour
{
    public AudioClip soundClip;                         // 재생할 효과음
    public AudioMixerGroup outputGroup;                 // 오디오 믹서 그룹
    public float triggerRadius = 10f;                   // 최대 감지 거리
    public LayerMask obstacleLayer;                     // 장애물 레이어
    public Transform player;                            // 플레이어 Transform

    private bool hasPlayed = false;                     // 중복 재생 방지

    void Update()
    {
        if (hasPlayed) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerRadius)
        {
            // 플레이어와의 직선 거리 상에 장애물이 있는지 Raycast로 확인
            Vector3 direction = (player.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, player.position);

            if (!Physics.Raycast(transform.position, direction, dist, obstacleLayer))
            {
                PlaySound();
                hasPlayed = true;
            }
        }
    }

    void PlaySound()
    {
        AudioSource audio = new GameObject("3DSound").AddComponent<AudioSource>();
        audio.transform.position = transform.position;
        audio.clip = soundClip;
        audio.outputAudioMixerGroup = outputGroup;
        audio.spatialBlend = 1f;         // 3D 사운드
        audio.minDistance = 1f;
        audio.maxDistance = triggerRadius;
        audio.Play();

        Destroy(audio.gameObject, soundClip.length + 1f);
    }
}