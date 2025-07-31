using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_SignUp : MonoBehaviour
{
    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passInput;
    [SerializeField] TMP_InputField passConfirmInput;

    [SerializeField] Button btn_SignUp;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_SignUp.onClick.AddListener(SignUp);
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void SignUp()
    {
        if (passInput.text != passConfirmInput.text)
        {
            Debug.LogError("패스워드가 일치하지 않습니다");
            return;
        }

        // 유니티는 ContinueWithOnMainThread를 사용해야함
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(idInput.text, passInput.text).ContinueWithOnMainThread(task =>
        {
            // 가입이 취소된 경우
            if (task.IsCanceled)
            {
                Debug.LogError("이메일 가입 취소됨");
                return;
            }

            // 가입이 실패한 경우
            if (task.IsFaulted)
            {
                Debug.LogError($"이메일 가입 실패함. 실패사유: {task.Exception}, ErrorCode: {((Firebase.FirebaseException)task.Exception.InnerException).ErrorCode}");
                return;
            }

            // 가입이 성공적으로 되었을 경우
            AuthResult result = task.Result;
            Debug.Log("이메일 가입 성공!");
            UIManager.Instance.ClosePanel(gameObject);
        });
    }
}
