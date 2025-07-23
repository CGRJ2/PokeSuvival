using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyDefault : MonoBehaviour
{
    [SerializeField] Button btn_QuickMatch;
    [SerializeField] Button btn_MatchMaking;

    public void Init()
    {
        btn_QuickMatch.onClick.AddListener(QuickMatch);
    }

    public void QuickMatch()
    {
        if (PhotonNetwork.LocalPlayer.IsLocal)
            PhotonNetwork.LoadLevel("Test_InGameScene(CJM)");
    }
}
