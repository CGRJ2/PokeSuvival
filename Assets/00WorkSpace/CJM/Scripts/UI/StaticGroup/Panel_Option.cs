using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Option : MonoBehaviour
{
    [SerializeField] Button btn_Esc;

    public void Init()
    {
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }
}
