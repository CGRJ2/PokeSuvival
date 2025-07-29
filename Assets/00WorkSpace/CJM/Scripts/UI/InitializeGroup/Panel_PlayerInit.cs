using Photon.Pun;
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
        if (BackendManager.Auth.CurrentUser != null)
        {
            // Auth 유저 프로필 업데이트
            BackendManager.Instance.UpdateUserProfile(inputField_Name.text);
            
            // DB에 저장
            BackendManager.Instance.InitUserDataToDB(new UserData(inputField_Name.text));
        }

        // 포톤에 닉네임 설정
        PhotonNetwork.NickName = inputField_Name.text;
        PhotonNetwork.JoinLobby();
        gameObject.SetActive(false);
    }
}
