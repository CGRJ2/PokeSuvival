using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_UpperMenu : MonoBehaviour
{
    [SerializeField] Button btn_Option;
    [SerializeField] Button btn_Lobby;
    [SerializeField] Button btn_DropDown;
    [SerializeField] TMP_Text tmp_State;

    RectTransform rect;
    [SerializeField] Vector2 closePos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float duration;


    bool isOpened = false;

    public void Init()
    {
        rect = GetComponent<RectTransform>();

        btn_Option.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_Option.gameObject));
        btn_Lobby.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.InGameGroup.panel_ReturnToLobbyConfirm.gameObject));
        btn_DropDown.onClick.AddListener(SwitchToggleDropDownButton);
    }

    private void SwitchToggleDropDownButton()
    {

        if (isOpened)
        {
            tmp_State.text = $"{NetworkManager.Instance.CurServer.name}";
            rect.DOAnchorPos(closePos, duration).SetEase(Ease.OutCubic);
            isOpened = false;
        }
        else
        {
            rect.DOAnchorPos(openPos, duration).SetEase(Ease.OutCubic);
            isOpened = true;
        }
    }


}
