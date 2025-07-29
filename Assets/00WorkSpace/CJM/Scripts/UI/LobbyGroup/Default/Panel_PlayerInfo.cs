using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PlayerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Email;
    [SerializeField] TMP_Text tmp_UserId;

    [SerializeField] Button btn_LogOut;
    [SerializeField] Button btn_EditProfile;
    [SerializeField] GameObject panel_Loading;

    public void Init()
    {
        btn_LogOut.onClick.AddListener(LogOut);
        btn_EditProfile.onClick.AddListener(EditProfile);
    }



    public void UpdateView()
    {
        StartCoroutine(WaitUntilUserInfoLoaded());
    }

    private IEnumerator WaitUntilUserInfoLoaded()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        panel_Loading.SetActive(true);
        yield return new WaitUntil(() => user.DisplayName != "");

        tmp_Name.text = user.DisplayName;
        tmp_Email.text = user.Email;
        tmp_UserId.text = user.UserId;
        panel_Loading.SetActive(false);
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
