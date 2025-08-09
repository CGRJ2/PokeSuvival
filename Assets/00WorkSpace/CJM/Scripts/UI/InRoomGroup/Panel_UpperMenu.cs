using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_UpperMenu : MonoBehaviour
{
    public Panel_ReturnToLobbyConfirm panel_ReturnToLobbyConfirm;

    [SerializeField] Button btn_Lobby;
    [SerializeField] Button btn_Dead;

    [SerializeField] Button btn_Option;
    [SerializeField] Button btn_Quit;
    [SerializeField] Button btn_DropDown;
    [SerializeField] TMP_Text tmp_State;

    RectTransform rect;
    [SerializeField] Vector2 closePos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float duration;


    bool isOpened = false;

    public void Init()
    {
        panel_ReturnToLobbyConfirm.Init();

        rect = GetComponent<RectTransform>();

        btn_Option.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.StaticGroup.panel_Option.gameObject));
        btn_Quit.onClick.AddListener(() => NetworkManager.Instance.GameQuit());

        btn_Lobby.onClick.AddListener(() => UIManager.Instance.OpenPanel(panel_ReturnToLobbyConfirm.gameObject));
        

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

                btn_Dead.gameObject.SetActive(false);
                btn_Dead.onClick.RemoveAllListeners();
            }
            else
            {
                btn_Lobby.gameObject.SetActive(true);

                PlayerController pc = PlayerManager.Instance?.LocalPlayerController;
                if (pc == null) return;
                
                Debug.Log(pc.Model.IsDead);

                if (!pc.Model.IsDead)
                {
                    btn_Dead.gameObject.SetActive(true);
                    btn_Dead.onClick.AddListener(() =>
                    {
                        //pc.Model.OnDied?.Invoke();
                        pc.Model.SetCurrentHp(-1);
                        SwitchToggleDropDownButton();
                        btn_Dead.onClick.RemoveAllListeners();
                    });
                }
                else
                {
                    btn_Dead.gameObject.SetActive(false);
                }
            }

            tmp_State.text = $"{NetworkManager.Instance.CurServer.name}";

            rect.DOAnchorPos(openPos, duration).SetEase(Ease.OutCubic);
            isOpened = true;
        }
    }

    

}
