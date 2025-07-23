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
        // �ϴ� �̷��� �صΰ�, Ư�� ������ �� ǥ���� �޼��� ���߿� ��������.
        tmp_Loading.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}
