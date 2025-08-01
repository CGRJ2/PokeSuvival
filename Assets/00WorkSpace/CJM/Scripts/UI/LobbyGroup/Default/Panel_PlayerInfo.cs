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
    
    [SerializeField] Button btn_LogIn;

    [SerializeField] GameObject panel_Loading;
    [SerializeField] Panel_EditProfile panel_EditProfile;


    public void Init()
    {
        panel_EditProfile.Init();

        btn_LogOut.onClick.AddListener(LogOut);
        btn_LogIn.onClick.AddListener(OpenLonInPanel);
        btn_EditProfile.onClick.AddListener(() => UIManager.Instance.OpenPanel(panel_EditProfile.gameObject));
    }

    private IEnumerator WaitUntilUserInfoLoaded()
    {
        // �̷� WaitUntil�� ���� ����ϴ� �������� �ݹ� ������� �����丵���ִ� �۾� �ʿ� (TODO)


        FirebaseUser user = BackendManager.Auth.CurrentUser;
        panel_Loading.SetActive(true);
        Debug.Log("���� �̸� ���� �ȵǾ ��ٸ�");
        yield return new WaitUntil(() => !user.DisplayName.IsNullOrEmpty());
        Debug.Log("���� �����Ұ� ��ٸ�����");
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
        tmp_Email.text = "�÷��̵����� ������ ����\n�α����� �ʿ��մϴ�.";
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
        // �ӽ�
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.panel_LogIn.gameObject);
    }

    private void LogOut()
    {
        BackendManager.Auth.SignOut();
        PhotonNetwork.LocalPlayer.CustomProperties = new ExitGames.Client.Photon.Hashtable(); // Ŀ���� ������Ƽ �ʱ�ȭ
        UIManager um = UIManager.Instance;
        um.ClosePanel(um.LobbyGroup.gameObject);
        um.OpenPanel(um.InitializeGroup.gameObject);
    }

    
}
