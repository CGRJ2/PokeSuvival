using Photon.Pun;
using UnityEngine;

public class EffectAutoDestroy : MonoBehaviourPun
{
    public void DestroyEffect()
    {
        if (!photonView.IsMine) return;
        Debug.Log("이펙트 파괴");
        PhotonNetwork.Destroy(gameObject);
    }
}
