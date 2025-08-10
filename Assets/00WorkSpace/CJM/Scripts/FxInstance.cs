using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxInstance : MonoBehaviourPun
{
    private AudioSource audioSource;
    private Transform target;      // 따라갈 투사체
    private bool following = true; // 타깃 살아있는 동안만 추적

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource) { audioSource.playOnAwake = false; audioSource.loop = false; }
    }

    [PunRPC]
    public void SetTarget(int targetViewID)
    {
        var pv = PhotonView.Find(targetViewID);
        if (pv != null) target = pv.transform;
        else target = null;

        AudioSource targetAudio = pv.transform.GetComponent<AudioSource>();
        audioSource.clip = targetAudio.clip;
        audioSource.Play();
    }

    [PunRPC]
    public void UpdatePos()
    {
        // 타깃이 존재할 때만 위치/회전 추적
        if (following && target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation; // 회전도 맞추고 싶으면 유지
        }
        else
        {
            following = false; // 타깃이 사라졌으면 그 자리에서 정지
        }
    }


    void LateUpdate()
    {
        photonView.RPC(nameof(UpdatePos), RpcTarget.All);
    }

    void Update()
    {
        if (audioSource == null) { if (photonView.IsMine) PhotonNetwork.Destroy(gameObject); return; }

        // 한번이라도 재생이 시작된 후 멈추면 네트워크로 제거(소유자만)
        if (!audioSource.isPlaying && audioSource.time > 0f)
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }
}
