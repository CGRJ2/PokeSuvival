using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 

public class ItemUI : MonoBehaviour // 아이템 패널 UI 제어 클래스
{
    public Image itemImage; // 아이템 이미지를 표시할 Image 컴포넌트
    public TMP_Text itemNameText; // 아이템 이름을 표시할 텍스트 컴포넌트
    public TMP_Text itemPriceText; // 아이템 가격을 표시할 텍스트 컴포넌트
    public TMP_Text itemDescriptionText; // 아이템 설명을 표시할 텍스트

    private Button panelButton; // 패널 클릭용 Button 컴포넌트 참조
    private System.Action onClickAction; // 클릭 시 실행할 델리게이트(이벤트)

    void Awake() // 오브젝트가 생성될 때 호출
    {
        panelButton = GetComponentInChildren<Button>(); // 자식에서 Button 컴포넌트 찾기
        if (panelButton != null) // Button이 있으면
            panelButton.onClick.AddListener(OnPanelClick); // 클릭 이벤트 등록
    }

    // 아이템 정보를 UI에 표시하고 클릭 이벤트를 설정하는 함수
    public void Setup(Sprite sprite, string name, float price, string description, System.Action onClick)
    {
        itemImage.sprite = sprite; // 이미지 설정
        itemNameText.text = name; // 이름 텍스트 설정
        itemPriceText.text = price.ToString(); // 가격 텍스트 설정
        itemDescriptionText.text = description; // 설명 표시
        onClickAction = onClick; // 클릭 시 실행할 델리게이트 저장


    }

    // 패널이 클릭되었을 때 호출되는 함수
    private void OnPanelClick()
    {
        onClickAction?.Invoke(); // 클릭 델리게이트가 있으면 실행
    }
}