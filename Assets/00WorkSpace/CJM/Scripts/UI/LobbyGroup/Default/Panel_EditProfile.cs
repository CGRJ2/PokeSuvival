using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_EditProfile : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField_EditName;
    [SerializeField] Button btn_Confirm;
    [SerializeField] Button btn_Cancel;

    public void Init()
    {
        btn_Confirm.onClick.AddListener(EditProfile);
        btn_Cancel.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void EditProfile()
    {
        if (string.IsNullOrWhiteSpace(inputField_EditName.text))
        {
            Debug.LogError("닉네임을 제대로 설정해주세요");
            return;
        }

        // 로그인한 상태라면 => Auth 사용자 프로필에 업데이트
        if (BackendManager.Auth.CurrentUser != null)
        {
            // Auth => 이거 DB에서 이름 바뀌었을 때 구독해놓는 식으로 하면 될듯?
            BackendManager.Instance.UpdateUserProfile(inputField_EditName.text);

            // DB
            BackendManager.Instance.UpdateUserDataValue("name", inputField_EditName.text);
        }

        PhotonNetwork.NickName = inputField_EditName.text;
        
        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdatePlayerInfoView();
        UIManager.Instance.ClosePanel(gameObject);
    }
}
