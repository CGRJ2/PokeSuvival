using Photon.Pun;
using UnityEngine;

public class SkillFXControl : MonoBehaviourPun
{
    AudioSource audioSource;
    private void OnEnable()
    {
        if (!photonView.IsMine) return; // �����ڸ� ����

        audioSource = GetComponent<AudioSource>();

        GameObject soundPrefab = PhotonNetwork.Instantiate("SoundInstancePrefab/FxSoundInstance", transform.position, transform.rotation);

        // ���� �ν��Ͻ��� "��(����ü)�� ���󰡶�" ����
        var sfxPv = soundPrefab.GetComponent<PhotonView>();
        sfxPv.RPC(nameof(FxInstance.SetTarget), RpcTarget.All, photonView.ViewID);

        // ���(���ÿ����� Ŭ���� �ȾƼ� �÷���)
        // ��� Ŭ�󿡼� ������ Ŭ���� �ʿ��ϸ�, Ű�� RPC�� ������ FxInstance���� ã�� ����.
        /*var audioSourceInstance = soundPrefab.GetComponent<AudioSource>();
        audioSourceInstance.clip = audioSource.clip;
        audioSourceInstance.Play();*/
    }

    
}
