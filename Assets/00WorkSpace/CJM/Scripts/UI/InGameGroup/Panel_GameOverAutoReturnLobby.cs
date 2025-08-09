using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Panel_GameOverAutoReturnLobby : MonoBehaviour
{
    [SerializeField] float lobbyMoveWaitTime;

    private void OnEnable()
    {
        StartCoroutine(WaitAndMoveToLobby());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    IEnumerator WaitAndMoveToLobby()
    {
        yield return new WaitForSeconds(lobbyMoveWaitTime);
        NetworkManager.Instance.MoveToLobby();
        gameObject.SetActive(false);
    }
}
