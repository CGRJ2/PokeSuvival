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
        UIManager.Instance.InGameGroup.panel_GameOver.gameObject.SetActive(false);
        gameObject.SetActive(false);
        NetworkManager.Instance.MoveToLobby();
    }
}
