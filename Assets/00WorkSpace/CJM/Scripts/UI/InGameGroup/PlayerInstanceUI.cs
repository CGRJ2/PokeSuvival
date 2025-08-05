using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstanceUI : MonoBehaviour
{
    PlayerController pc;
    [SerializeField] PlayerHeadUI headUI;
    [SerializeField] Image image_MiniMapPoint;

    private void Awake() => Init();

    public void Init()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (pc.Model != null)
        {
            headUI.UpdateView(pc.Model);

            // 본인이라면
            if (PhotonNetwork.NickName == pc.Model.PlayerName)
            {
                image_MiniMapPoint.color = Color.green;
            }
            // 적들이라면
            else
            {
                image_MiniMapPoint.color = Color.red;
            }
        }
            
    }

    private void OnEnable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.activedPlayerList.Add(pc);
    }
    private void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.activedPlayerList.Remove(pc);
    }

}
