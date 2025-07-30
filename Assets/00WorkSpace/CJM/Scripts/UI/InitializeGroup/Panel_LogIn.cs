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
            // 로그인 도중에 취소된 상황
            if (task.IsCanceled)
            {
                Debug.LogError("로그인이 취소됨");
                return;
            }

            // 로그인이 실패한 상황
            if (task.IsFaulted)
            {
                Debug.LogError($"로그인이 실패함. 실패사유: {task.Exception}, ErrorCode: {((FirebaseException)task.Exception.InnerException).ErrorCode}");
                return;
            }

            // 로그인 성공
            Firebase.Auth.AuthResult result = task.Result;
            Debug.Log($"성공적으로 로그인 됨: {result.User.DisplayName} (UserId: {result.User.UserId}) / (Email: {result.User.Email})");


            UIManager um = UIManager.Instance;
            FirebaseUser user = task.Result.User;
            
            // 처음 생성한 플레이어라면 => 닉네임 설정 패널 열기
            if (user.DisplayName.IsNullOrEmpty())
            {
                um.ClosePanel(gameObject);
                um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
            }
            // 만약 플레이어 데이터가 이미 존재한다면 => 로비로 이동
            else
            {
                um.ClosePanel(gameObject);
                PhotonNetwork.JoinLobby();
            }
        });

        
        
    }
}
