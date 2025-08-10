using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            
            // ���� ������ ���ӵ� �Խ�ƮId�� �α�������Id�� �ٲ��ֱ�
            //BackendManager.Instance.OnExitServerCapacityUpdate(NetworkManager.Instance.CurServer, $"Guest({PhotonNetwork.LocalPlayer.UserId})");
            //BackendManager.Instance.OnEnterServerCapacityUpdate(NetworkManager.Instance.CurServer, new List<string>() { BackendManager.Auth.CurrentUser.UserId });

            UIManager um = UIManager.Instance;
            FirebaseUser user = task.Result.User;

            // ���� ������ �ҷ�����
            BackendManager.Instance.LoadUserDataFromDB(
            (userData) =>
            {
                // ������ �ҷ����� ���� �� (= �÷��̾� �����Ͱ� �̹� �����Ѵٴ� ��)
                // �ҷ����µ��� �����ص� �̸� ������ ���� ���¶�� => �÷��̾� ������ ���� ����
                if (string.IsNullOrEmpty(userData.name))
                {
                    um.ClosePanel(gameObject);
                    um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
                }
                // �̹� �����͸� �����ص� �÷��̾� ��� => �ٷ� �κ�� �̵�
                else
                {
                    Debug.Log("���� ������ ����ȭ ����");
                    // ������ ���������͸� Ŭ���̾�Ʈ�� ����ȭ
                    NetworkManager.Instance.UpdateUserDataToClient(userData);

                    // Ŀ���� ������Ƽ ����ȭ �Ϸ� �� ���� ����ߴٰ� �κ�� �̵�
                    StartCoroutine(JoinLobbyAfterDataUpdated());
                }
            },
            (message) =>
            {
                // ������ �ҷ����� ���� ��

                // ���� ���� ���� ���� => �÷��̾� ������ ���� ����
                um.ClosePanel(gameObject);
                um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
            });
        });
    }

    private IEnumerator JoinLobbyAfterDataUpdated()
    {
        // �̷� WaitUntil�� ���� ����ϴ� �������� �ݹ� ������� �����丵���ִ� �۾� �ʿ� (TODO)
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] != null);
        PhotonNetwork.JoinLobby();
        UIManager.Instance.ClosePanel(gameObject);
    }

    private void OnDestroy() => StopAllCoroutines();

}
