using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeadUI : MonoBehaviour
{
    [SerializeField] Image image_HPBar;
    [SerializeField] TMP_Text tmp_NickName;
    [SerializeField] TMP_Text tmp_Level;

    public void UpdateView(PlayerModel model)
    {
        tmp_NickName.text = $"{model.PlayerName}";
        tmp_Level.text = $"{model.PokeLevel}";

        image_HPBar.fillAmount = Mathf.Clamp01((float)model.CurrentHp / (float)model.MaxHp);

        // 본인이라면
        if (model.UserId == NetworkManager.Instance.GetUserId())
        {
            image_HPBar.color = Color.green;
        }
        // 적들이라면
        else
        {
            image_HPBar.color = Color.red;
        }
    }
}
