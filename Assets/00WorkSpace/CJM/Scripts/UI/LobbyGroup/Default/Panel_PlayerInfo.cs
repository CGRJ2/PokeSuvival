using Firebase.Auth;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Panel_PlayerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Email;
    [SerializeField] TMP_Text tmp_UserId;

    [SerializeField] Button btn_LogOut;
    [SerializeField] Button btn_EditProfile;
    [SerializeField] GameObject panel_Loading;

    [SerializeField] Button btn_LogIn;


    public void Init()
    {
        btn_LogOut.onClick.AddListener(LogOut);
        btn_EditProfile.onClick.AddListener(EditProfile);
        btn_LogIn.onClick.AddListener(OpenLonInPanel);
    }

    private IEnumerator WaitUntilUserInfoLoaded()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        panel_Loading.SetActive(true);
        yield return new WaitUntil(() => !user.DisplayName.IsNullOrEmpty());

        tmp_Name.text = $"Name: {user.DisplayName}";
        tmp_Email.text = $"E-Mail: {user.Email}";
        tmp_UserId.text = $"User Id: {user.UserId}";
        panel_Loading.SetActive(false);
    }

    public void UpdatePlayerInfoView()
    {
        btn_LogIn.interactable = false;
        btn_LogOut.interactable = true;
        btn_EditProfile.interactable = true;
        btn_LogIn.gameObject.SetActive(false);

        StartCoroutine(WaitUntilUserInfoLoaded());
    }

    public void UpdateGuestInfoView()
    {
        tmp_Name.text = $"Name: {PhotonNetwork.LocalPlayer.NickName}";
        tmp_Email.text = "플레이데이터 저장을 위해\n로그인이 필요합니다.";
        tmp_UserId.text = "";
        btn_LogIn.interactable = true;
        btn_LogOut.interactable = false;
        btn_EditProfile.interactable = false;
        btn_LogIn.gameObject.SetActive(true);
    }
    public void ClearView()
    {
        tmp_Name.text = "Name: ";
        tmp_Email.text = "E-Mail: ";
        tmp_UserId.text = "User Id: ";
    }

    private void OpenLonInPanel()
    {
        // 임시
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.panel_LogIn.gameObject);
    }

    private void LogOut()
    {
        BackendManager.Auth.SignOut();
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.gameObject);
    }

    private void EditProfile()
    {

    }
}
