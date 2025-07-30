using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Panel_UpperMenu : MonoBehaviour
{
    [SerializeField] Button btn_Option;
    [SerializeField] Button btn_Lobby;
    [SerializeField] Button btn_DropDown;

    RectTransform rect;
    [SerializeField] Vector2 closePos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float duration;


    bool isOpened = false;

    public void Init()
    {
        rect = GetComponent<RectTransform>();

        btn_Option.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_Option.gameObject));
        btn_Lobby.onClick.AddListener(() => NetworkManager.Instance.MoveToLobby());
        btn_DropDown.onClick.AddListener(SwitchToggleDropDownButton);
    }

    private void SwitchToggleDropDownButton()
    {

        if (isOpened)
        {
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
