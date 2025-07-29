using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PlayerInit : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField_Name;
    [SerializeField] Button btn_Create;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_Create.onClick.AddListener(NicknameAdmit);
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }


    public void NicknameAdmit()
    {
        if (string.IsNullOrWhiteSpace(inputField_Name.text))
        {
            Debug.LogError("닉네임을 제대로 설정해주세요");
            return;
        }

        // 로그인한 상태라면 => Auth 사용자 프로필에 업데이트
        if (BackendManager.Auth.CurrentUser.IsValid())
        {
            UpdateUserProfile(inputField_Name.text);
        }

        // 포톤에 닉네임 설정
        PhotonNetwork.NickName = inputField_Name.text;
        PhotonNetwork.JoinLobby();
        gameObject.SetActive(false);
    }

    public void UpdateUserProfile(string name)
    {
        Firebase.Auth.FirebaseUser user = BackendManager.Auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = name,
                //PhotoUrl = new System.Uri("https://example.com/jane-q-user/profile.jpg"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }
    }
}
