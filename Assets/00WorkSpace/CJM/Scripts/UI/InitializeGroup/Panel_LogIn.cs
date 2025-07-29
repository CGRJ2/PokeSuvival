using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Panel_LogIn : MonoBehaviour
{
    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passInput;

    [SerializeField] Button btn_LogIn;
    [SerializeField] Button btn_SignUp;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_LogIn.onClick.AddListener(LogIn);
        btn_SignUp.onClick.AddListener(() => UIManager.Instance.OpenPanel(UIManager.Instance.InitializeGroup.panel_SignUp.gameObject));
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void LogIn()
    {
        BackendManager.Auth.SignInWithEmailAndPasswordAsync(idInput.text, passInput.text).ContinueWithOnMainThread(task =>
        {
            // �α��� ���߿� ��ҵ� ��Ȳ
            if (task.IsCanceled)
            {
                Debug.LogError("�α����� ��ҵ�");
                return;
            }

            // �α����� ������ ��Ȳ
            if (task.IsFaulted)
            {
                Debug.LogError($"�α����� ������. ���л���: {task.Exception}, ErrorCode: {((FirebaseException)task.Exception.InnerException).ErrorCode}");
                return;
            }

            // �α��� ����
            Firebase.Auth.AuthResult result = task.Result;
            Debug.Log($"���������� �α��� ��: {result.User.DisplayName} (UserId: {result.User.UserId}) / (Email: {result.User.Email})");


            UIManager um = UIManager.Instance;
            FirebaseUser user = task.Result.User;
            
            // ó�� ������ �÷��̾��� => �г��� ���� �г� ����
            if (user.DisplayName.IsNullOrEmpty())
            {
                um.ClosePanel(gameObject);
                um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
            }
            // ���� �÷��̾� �����Ͱ� �̹� �����Ѵٸ� => �κ�� �̵�
            else
            {
                um.ClosePanel(gameObject);
                PhotonNetwork.JoinLobby();
            }
        });

        
        
    }
}
