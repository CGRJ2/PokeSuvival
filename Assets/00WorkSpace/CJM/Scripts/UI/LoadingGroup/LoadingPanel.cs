using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Loading;

    void Update()
    {
        UpdateCurState();
    }

    private void UpdateCurState()
    {
        // 일단 이렇게 해두고, 특정 상태일 때 표기할 메세지 나중에 정리하자.
        tmp_Loading.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}
