using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_QuitConfirm : MonoBehaviour
{
    [SerializeField] Button btn_Confirm;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_Confirm.onClick.AddListener(() => NetworkManager.Instance.GameQuit());
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }
}
