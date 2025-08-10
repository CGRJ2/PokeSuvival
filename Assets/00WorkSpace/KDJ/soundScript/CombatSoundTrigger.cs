using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSoundTrigger : MonoBehaviour
{

    public SoundPlayer attackSound;// 공격 사운드를 재생할 SoundPlayer
    public SoundPlayer hitSound; // 피격 사운드를 재생할 SoundPlayer

    public void PlayAttackSound()// 공격 시 호출되는 메서드: 공격 사운드 재생
    {
        attackSound.Play();
    }
    public void PlayHitSound()// 피격 시 호출되는 메서드: 피격 사운드 재생
    {
        hitSound.Play();
    }
}