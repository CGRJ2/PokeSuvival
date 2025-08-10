using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxInstance : MonoBehaviourPun
{
    private AudioSource audioSource;
    private Transform target;      // ���� ����ü
    private bool following = true; // Ÿ�� ����ִ� ���ȸ� ����

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
        // Ÿ���� ������ ���� ��ġ/ȸ�� ����
        if (following && target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation; // ȸ���� ���߰� ������ ����
        }
        else
        {
            following = false; // Ÿ���� ��������� �� �ڸ����� ����
        }
    }


    void LateUpdate()
    {
        photonView.RPC(nameof(UpdatePos), RpcTarget.All);
    }

    void Update()
    {
        if (audioSource == null) { if (photonView.IsMine) PhotonNetwork.Destroy(gameObject); return; }

        // �ѹ��̶� ����� ���۵� �� ���߸� ��Ʈ��ũ�� ����(�����ڸ�)
        if (!audioSource.isPlaying && audioSource.time > 0f)
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }
}
