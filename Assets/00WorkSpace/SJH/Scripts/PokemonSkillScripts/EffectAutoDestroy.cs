using Photon.Pun;
using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    public void DestroyEffect()
    {
        Debug.Log("이펙트 파괴");
        PhotonNetwork.Destroy(gameObject);
    }
}
