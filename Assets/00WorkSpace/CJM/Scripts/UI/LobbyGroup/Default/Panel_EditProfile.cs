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
            Debug.LogError("�г����� ����� �������ּ���");
            return;
        }

        // �α����� ���¶�� => Auth ����� �����ʿ� ������Ʈ
        if (BackendManager.Auth.CurrentUser != null)
        {
            // Auth => �̰� DB���� �̸� �ٲ���� �� �����س��� ������ �ϸ� �ɵ�?
            BackendManager.Instance.UpdateUserProfile(inputField_EditName.text);

            // DB
            BackendManager.Instance.UpdateUserDataValue("name", inputField_EditName.text);
        }

        PhotonNetwork.NickName = inputField_EditName.text;
        
        UIManager.Instance.LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdatePlayerInfoView();
        UIManager.Instance.ClosePanel(gameObject);
    }
}
