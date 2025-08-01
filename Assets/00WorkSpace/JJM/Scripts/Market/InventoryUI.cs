using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NTJ;

public class InventoryUI : MonoBehaviour // 인벤토리(도감) UI 관리 클래스
{
    [Header("타이틀 및 선택 아이템 패널")]
    public TMP_Text titleText; // Title Panel의 Title Text (TMP)
    public Image chooseItemImage; // 선택된 아이템의 이미지
    public TMP_Text chooseItemNameText; // 선택된 아이템의 이름
    public TMP_Text chooseItemDescriptionText; // 선택된 아이템의 설명

    [Header("아이템 리스트 패널")]
    public List<Image> itemSlotImages; // 아이템 리스트 패널의 각 아이템 이미지 슬롯

    [Header("페이지네이션")]
    public Image leftArrowImage; // 왼쪽 화살표 이미지
    public Image rightArrowImage; // 오른쪽 화살표 이미지
    public TMP_Text pageText; // 현재 페이지/전체 페이지 표시

    public int itemsPerPage = 6; // 한 페이지에 보여줄 아이템 개수
    private int currentPage = 0; // 현재 페이지(0부터 시작)
    private int totalPages = 1; // 전체 페이지 수

    [Header("데이터")]
    public List<ItemData> allItems; // 전체 아이템 데이터 리스트
    public HashSet<int> ownedItemIds = new HashSet<int>(); // 구매한 아이템 id 집합

    [Header("미획득 안내 UI")]
    public GameObject notOwnedPanel; // "아직 획득하지 않았습니다" 안내 UI

    [Header("인벤토리 루트 오브젝트")]
    public GameObject inventoryRootPanel; // 인벤토리 전체 오브젝트



    public static InventoryUI Instance; // 싱글톤 인스턴스

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        totalPages = Mathf.CeilToInt((float)allItems.Count / itemsPerPage); // 전체 페이지 계산
        UpdatePage(); // 첫 페이지 표시
    }
    void Update()
    {
        // I 키를 누르면 인벤토리 UI 토글
        if (Input.GetKeyDown(KeyCode.I) && inventoryRootPanel != null)
        {
            inventoryRootPanel.SetActive(!inventoryRootPanel.activeSelf);
        }
    }

    public void UpdatePage()
    {
        // 페이지 텍스트 갱신
        pageText.text = $"{currentPage + 1}/{totalPages}";

        // 좌/우 화살표 버튼의 interactable만 조절 (색상 변경 X)
        var leftBtn = leftArrowImage.GetComponent<Button>();
        if (leftBtn != null)
            leftBtn.interactable = currentPage > 0;

        var rightBtn = rightArrowImage.GetComponent<Button>();
        if (rightBtn != null)
            rightBtn.interactable = currentPage < totalPages - 1;

        // 아이템 슬롯 갱신
        for (int i = 0; i < itemSlotImages.Count; i++)
        {
            int itemIdx = currentPage * itemsPerPage + i;
            if (itemIdx < allItems.Count)
            {
                var item = allItems[itemIdx];
                bool owned = ownedItemIds.Contains(item.id);
                itemSlotImages[i].sprite = item.sprite; // 항상 원본 스프라이트 사용
                itemSlotImages[i].color = owned ? Color.white : Color.black; // 소유 여부에 따라 밝기 변경

                // 클릭 이벤트 등록
                int idx = itemIdx; // 클로저 문제 방지
                var btn = itemSlotImages[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSlotClick(idx));
            }
            else
            {
                itemSlotImages[i].sprite = null; // 빈 슬롯
                itemSlotImages[i].color = new Color(1, 1, 1, 0); // 투명 처리
                var btn = itemSlotImages[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    public void OnItemSlotClick(int itemIdx)
    {
        var item = allItems[itemIdx];
        bool owned = ownedItemIds.Contains(item.id);
        if (owned)
        {
            // 아이템 정보 표시
            chooseItemImage.sprite = item.sprite;
            chooseItemNameText.text = item.itemName;
            chooseItemDescriptionText.text = item.description;
        }
        else
        {
            // 미획득 안내 UI 표시
            if (notOwnedPanel != null)
            {
                notOwnedPanel.SetActive(true);
                StartCoroutine(HideNotOwnedPanel());
            }
        }
    }

    private System.Collections.IEnumerator HideNotOwnedPanel()
    {
        yield return new WaitForSeconds(2f); // 2초 대기
        if (notOwnedPanel != null)
            notOwnedPanel.SetActive(false); // 안내 UI 끄기
    }

    public void OnLeftArrowClick()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    public void OnRightArrowClick()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }
}