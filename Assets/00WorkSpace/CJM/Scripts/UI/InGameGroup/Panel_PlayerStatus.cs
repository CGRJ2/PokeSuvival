using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PlayerStatus : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Level;
    [SerializeField] TMP_Text tmp_CurHP;
    [SerializeField] TMP_Text tmp_MaxHP;
    [SerializeField] Image image_HPBar;
    [SerializeField] Image image_EXPBar;

    public void Init()
    {

    }
}
