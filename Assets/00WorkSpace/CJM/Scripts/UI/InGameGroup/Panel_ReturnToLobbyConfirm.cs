using System.Collections;
using Unity.VisualScripting;
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
            UIManager.Instance.ClosePanel(gameObject);
            UIManager.Instance.StaticGroup.panel_UpperMenu.SwitchToggleDropDownButton();
            UIManager.Instance.InGameGroup.panel_HUD.panel_BuffState.InitSlots();
            UIManager.Instance.InGameGroup.panel_GameOverAutoReturnLobby.gameObject.SetActive(true);
        });
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void OnEnable()
    {
        PlayerController pc = PlayerManager.Instance?.LocalPlayerController;
        if (pc == null) return;

        Debug.Log(pc.Model.IsDead);

        if (!pc.Model.IsDead)
        {
            btn_Confirm.onClick.AddListener(Dead);
        }
        else
        {
            btn_Confirm.onClick.AddListener(JustMoveToLobby);
        }
    }
    private void OnDisable()
    {
        PlayerController pc = PlayerManager.Instance?.LocalPlayerController;
        if (pc == null) return;

        Debug.Log(pc.Model.IsDead);

        if (!pc.Model.IsDead)
        {
            btn_Confirm.onClick.RemoveListener(Dead);
        }
        else
        {
            btn_Confirm.onClick.RemoveListener(JustMoveToLobby);
        }
    }

    public void Dead()
    {
        PlayerController pc = PlayerManager.Instance?.LocalPlayerController;
        pc?.Model.SetCurrentHp(-1);
    }

    public void JustMoveToLobby()
    {
        NetworkManager.Instance.MoveToLobby();
    }
}
