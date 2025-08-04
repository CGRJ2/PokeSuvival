using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// 아이템에 붙이는 스크립트: 플레이어가 가까이 왔을 때 사운드를 재생 거리 기반 처리 포함
public class ItemPickupSound : MonoBehaviour
{
    public AudioClip pickupClip;// 아이템 획득 시 재생할 오디오 클립
    public AudioMixerGroup fxGroup;// FX 믹서 그룹 효과음용.
    private void OnTriggerEnter2D(Collider2D collision)// 플레이어가 2D 트리거에 진입할 때 호출
    {
        if (collision.CompareTag("Player"))// 플레이어와 충돌했는지 확인
        {
            // 새로운 GameObject를 만들어 거리 기반 사운드를 재생할 AudioSource 추가
            var audio = new GameObject("PickupSound").AddComponent<AudioSource>();

            audio.clip = pickupClip;// 클립 설정
            audio.outputAudioMixerGroup = fxGroup;// 믹서 그룹 설정 FX 그룹
            audio.spatialBlend = 1f;// 3D 사운드 설정 거리 기반
            audio.minDistance = 1f;// 소리 최대로 크게 들리는 거리
            audio.maxDistance = 10f;// 소리가 들릴 수 있는 최대 거리
            audio.Play();// 사운드 재생

            Destroy(audio.gameObject, 2f);// 2초 후 오디오 오브젝트 제거
        }
    }
}