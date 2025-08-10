using UnityEngine;
using UnityEngine.UI;

public class Panel_CautionNonePoke : MonoBehaviour
{
    [SerializeField] Button btn_ESC;

    public void Init()
    {
        btn_ESC.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }
}
