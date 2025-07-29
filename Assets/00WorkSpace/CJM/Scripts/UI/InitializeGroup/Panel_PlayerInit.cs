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
            Debug.LogError("�г����� ����� �������ּ���");
            return;
        }

        // �α����� ���¶�� => Auth ����� �����ʿ� ������Ʈ
        if (BackendManager.Auth.CurrentUser != null)
        {
            // Auth ���� ������ ������Ʈ
            BackendManager.Instance.UpdateUserProfile(inputField_Name.text);
            
            // DB�� ����
            BackendManager.Instance.InitUserDataToDB(new UserData(inputField_Name.text));
        }

        // ���濡 �г��� ����
        PhotonNetwork.NickName = inputField_Name.text;
        PhotonNetwork.JoinLobby();
        gameObject.SetActive(false);
    }
}
