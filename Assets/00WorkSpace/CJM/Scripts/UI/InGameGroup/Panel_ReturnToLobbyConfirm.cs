using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_ReturnToLobbyConfirm : MonoBehaviour
{
    [SerializeField] Button btn_Confirm;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_Confirm.onClick.AddListener(() =>
        {
            UIManager.Instance.StaticGroup.panel_UpperMenu.SwitchToggleDropDownButton();
            NetworkManager.Instance.MoveToLobby();
        });
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }
}
