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
            
            // 현재 서버에 접속된 게스트Id를 로그인유저Id로 바꿔주기
            //BackendManager.Instance.OnExitServerCapacityUpdate(NetworkManager.Instance.CurServer, $"Guest({PhotonNetwork.LocalPlayer.UserId})");
            //BackendManager.Instance.OnEnterServerCapacityUpdate(NetworkManager.Instance.CurServer, new List<string>() { BackendManager.Auth.CurrentUser.UserId });

            UIManager um = UIManager.Instance;
            FirebaseUser user = task.Result.User;

            // 유저 데이터 불러오기
            BackendManager.Instance.LoadUserDataFromDB(
            (userData) =>
            {
                // 데이터 불러오기 성공 시 (= 플레이어 데이터가 이미 존재한다는 뜻)
                // 불러오는데에 성공해도 이름 설정을 안한 상태라면 => 플레이어 데이터 생성 진행
                if (string.IsNullOrEmpty(userData.name))
                {
                    um.ClosePanel(gameObject);
                    um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
                }
                // 이미 데이터를 생성해둔 플레이어 라면 => 바로 로비로 이동
                else
                {
                    Debug.Log("유저 데이터 동기화 진행");
                    // 서버의 유저데이터를 클라이언트에 동기화
                    NetworkManager.Instance.UpdateUserDataToClient(userData);

                    // 커스텀 프로퍼티 동기화 완료 시 까지 대기했다가 로비로 이동
                    StartCoroutine(JoinLobbyAfterDataUpdated());
                }
            },
            (message) =>
            {
                // 데이터 불러오기 실패 시

                // 유저 정보 없음 판정 => 플레이어 데이터 생성 진행
                um.ClosePanel(gameObject);
                um.OpenPanel(um.InitializeGroup.panel_PlayerInit.gameObject);
            });
        });
    }

    private IEnumerator JoinLobbyAfterDataUpdated()
    {
        // 이런 WaitUntil로 무한 대기하는 구조들을 콜백 기반으로 리팩토링해주는 작업 필요 (TODO)
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties["StartingPokemon"] != null);
        PhotonNetwork.JoinLobby();
        UIManager.Instance.ClosePanel(gameObject);
    }

    private void OnDestroy() => StopAllCoroutines();

}
