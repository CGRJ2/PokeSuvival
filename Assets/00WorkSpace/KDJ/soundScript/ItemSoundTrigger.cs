using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSoundTrigger : MonoBehaviour
{
    public SoundPlayer soundPlayer;// 사운드를 재생할 SoundPlayer 스크립트 참조 (해당 오브젝트에 붙어 있어야 함)

    void OnTriggerEnter(Collider other)// 트리거 충돌 감지 다른 콜라이더가 이 오브젝트의 트리거에 닿았을 때 실행됨
    {
        if (other.CompareTag("Player"))// 닿은 오브젝트가 Player 태그를 가진 경우
        {
            soundPlayer.Play();// 사운드 재생
            // 이 부분에 아이템 획득 효과나 기능을 추가할 수 있음
            // 예: 체력 회복, 포인트 추가, 아이템 제거 등
        }
    }
}