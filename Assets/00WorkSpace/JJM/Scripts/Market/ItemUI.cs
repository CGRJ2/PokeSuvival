using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 

public class ItemUI : MonoBehaviour // ������ �г� UI ���� Ŭ����
{
    public Image itemImage; // ������ �̹����� ǥ���� Image ������Ʈ
    public TMP_Text itemNameText; // ������ �̸��� ǥ���� �ؽ�Ʈ ������Ʈ
    public TMP_Text itemPriceText; // ������ ������ ǥ���� �ؽ�Ʈ ������Ʈ
    public TMP_Text itemDescriptionText; // ������ ������ ǥ���� �ؽ�Ʈ

    private Button panelButton; // �г� Ŭ���� Button ������Ʈ ����
    private System.Action onClickAction; // Ŭ�� �� ������ ��������Ʈ(�̺�Ʈ)

    void Awake() // ������Ʈ�� ������ �� ȣ��
    {
        panelButton = GetComponentInChildren<Button>(); // �ڽĿ��� Button ������Ʈ ã��
        if (panelButton != null) // Button�� ������
            panelButton.onClick.AddListener(OnPanelClick); // Ŭ�� �̺�Ʈ ���
    }

    // ������ ������ UI�� ǥ���ϰ� Ŭ�� �̺�Ʈ�� �����ϴ� �Լ�
    public void Setup(Sprite sprite, string name, float price, string description, System.Action onClick)
    {
        itemImage.sprite = sprite; // �̹��� ����
        itemNameText.text = name; // �̸� �ؽ�Ʈ ����
        itemPriceText.text = price.ToString(); // ���� �ؽ�Ʈ ����
        itemDescriptionText.text = description; // ���� ǥ��
        onClickAction = onClick; // Ŭ�� �� ������ ��������Ʈ ����


    }

    // �г��� Ŭ���Ǿ��� �� ȣ��Ǵ� �Լ�
    private void OnPanelClick()
    {
        onClickAction?.Invoke(); // Ŭ�� ��������Ʈ�� ������ ����
    }
}