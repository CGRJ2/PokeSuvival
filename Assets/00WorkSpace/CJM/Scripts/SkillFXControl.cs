using Photon.Pun;
using UnityEngine;

public class SkillFXControl : MonoBehaviourPun
{
    AudioSource audioSource;
    private void OnEnable()
    {
        if (!photonView.IsMine) return; // 소유자만 생성

        audioSource = GetComponent<AudioSource>();

        GameObject soundPrefab = PhotonNetwork.Instantiate("SoundInstancePrefab/FxSoundInstance", transform.position, transform.rotation);

        // 사운드 인스턴스에 "나(투사체)를 따라가라" 지시
        var sfxPv = soundPrefab.GetComponent<PhotonView>();
        sfxPv.RPC(nameof(FxInstance.SetTarget), RpcTarget.All, photonView.ViewID);

        // 재생(로컬에서만 클립을 꽂아서 플레이)
        // 모든 클라에서 동일한 클립이 필요하면, 키를 RPC로 보내서 FxInstance에서 찾게 해줘.
        /*var audioSourceInstance = soundPrefab.GetComponent<AudioSource>();
        audioSourceInstance.clip = audioSource.clip;
        audioSourceInstance.Play();*/
    }

    
}
