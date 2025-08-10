using UnityEngine;

public class UIGroup_Initialize : MonoBehaviour
{
    public AudioClip InitializeDefaultBGM;

    public Panel_InitDefault panel_InitDefault;
    public Panel_PlayerInit panel_PlayerInit;
    public Panel_LogIn panel_LogIn;
    public Panel_SignUp panel_SignUp;

    private void OnEnable()
    {
        UIManager.Instance.StaticGroup.panel_CustomBGM.SetNewAudioClipAndPlay(InitializeDefaultBGM);
    }

    public void Init()
    {
        panel_InitDefault.Init();
        panel_PlayerInit.Init();
        panel_LogIn.Init();
        panel_SignUp.Init();
    }

    public void InitView()
    {
        gameObject.SetActive(true);
        panel_InitDefault.gameObject.SetActive(true);
    }
}
