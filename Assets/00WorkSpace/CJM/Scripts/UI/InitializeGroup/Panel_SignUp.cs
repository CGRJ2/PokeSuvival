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
            Debug.LogError("�н����尡 ��ġ���� �ʽ��ϴ�");
            return;
        }

        // ����Ƽ�� ContinueWithOnMainThread�� ����ؾ���
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(idInput.text, passInput.text).ContinueWithOnMainThread(task =>
        {
            // ������ ��ҵ� ���
            if (task.IsCanceled)
            {
                Debug.LogError("�̸��� ���� ��ҵ�");
                return;
            }

            // ������ ������ ���
            if (task.IsFaulted)
            {
                Debug.LogError($"�̸��� ���� ������. ���л���: {task.Exception}, ErrorCode: {((Firebase.FirebaseException)task.Exception.InnerException).ErrorCode}");
                return;
            }

            // ������ ���������� �Ǿ��� ���
            AuthResult result = task.Result;
            Debug.Log("�̸��� ���� ����!");
            UIManager.Instance.ClosePanel(gameObject);
        });
    }
}
