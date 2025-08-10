using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_UpperMenu : MonoBehaviour
{
    public Panel_ReturnToLobbyConfirm panel_ReturnToLobbyConfirm;

    [SerializeField] Button btn_Lobby;

    [SerializeField] Button btn_Option;
    [SerializeField] Button btn_Quit;
    [SerializeField] Button btn_DropDown;
    [SerializeField] Button btn_SetBGM;
    [SerializeField] TMP_Text tmp_State;
    [SerializeField] TMP_Text tmp_BGMState;

    RectTransform rect;
    [SerializeField] Vector2 closePos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float duration;


    bool isOpened = false;

    private void Update()
    {
        tmp_BGMState.text = $"BGM: {UIManager.Instance.StaticGroup.panel_CustomBGM.audioSource.clip?.name}";
    }

    public void Init()
    {
        panel_ReturnToLobbyConfirm.Init();

        rect = GetComponent<RectTransform>();

        btn_Option.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_Option.gameObject));
        btn_Quit.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_QuitConfirm.gameObject));
        btn_Lobby.onClick.AddListener(() => UIManager.Instance.OpenPanel(panel_ReturnToLobbyConfirm.gameObject));
        btn_SetBGM.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_CustomBGM.gameObject));
        btn_DropDown.onClick.AddListener(SwitchToggleDropDownButton);
    }

    public void SwitchToggleDropDownButton()
    {
        if (isOpened)
        {
            rect.DOAnchorPos(closePos, duration).SetEase(Ease.OutCubic);
            isOpened = false;
        }
        else
        {
            if (NetworkManager.Instance.CurServer.type != (int)ServerType.InGame)
            {
                btn_Lobby.gameObject.SetActive(false);
            }
            else
            {
                btn_Lobby.gameObject.SetActive(true);


            }

            tmp_State.text = $"현재 서버: {NetworkManager.Instance.CurServer.name}";

            rect.DOAnchorPos(openPos, duration).SetEase(Ease.OutCubic);
            isOpened = true;
        }
    }



}
