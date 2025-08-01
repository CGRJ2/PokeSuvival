using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NTJ;

public class InventoryUI : MonoBehaviour // �κ��丮(����) UI ���� Ŭ����
{
    [Header("Ÿ��Ʋ �� ���� ������ �г�")]
    public TMP_Text titleText; // Title Panel�� Title Text (TMP)
    public Image chooseItemImage; // ���õ� �������� �̹���
    public TMP_Text chooseItemNameText; // ���õ� �������� �̸�
    public TMP_Text chooseItemDescriptionText; // ���õ� �������� ����

    [Header("������ ����Ʈ �г�")]
    public List<Image> itemSlotImages; // ������ ����Ʈ �г��� �� ������ �̹��� ����

    [Header("���������̼�")]
    public Image leftArrowImage; // ���� ȭ��ǥ �̹���
    public Image rightArrowImage; // ������ ȭ��ǥ �̹���
    public TMP_Text pageText; // ���� ������/��ü ������ ǥ��

    public int itemsPerPage = 6; // �� �������� ������ ������ ����
    private int currentPage = 0; // ���� ������(0���� ����)
    private int totalPages = 1; // ��ü ������ ��

    [Header("������")]
    public List<ItemData> allItems; // ��ü ������ ������ ����Ʈ
    public HashSet<int> ownedItemIds = new HashSet<int>(); // ������ ������ id ����

    [Header("��ȹ�� �ȳ� UI")]
    public GameObject notOwnedPanel; // "���� ȹ������ �ʾҽ��ϴ�" �ȳ� UI

    [Header("�κ��丮 ��Ʈ ������Ʈ")]
    public GameObject inventoryRootPanel; // �κ��丮 ��ü ������Ʈ



    public static InventoryUI Instance; // �̱��� �ν��Ͻ�

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        totalPages = Mathf.CeilToInt((float)allItems.Count / itemsPerPage); // ��ü ������ ���
        UpdatePage(); // ù ������ ǥ��
    }
    void Update()
    {
        // I Ű�� ������ �κ��丮 UI ���
        if (Input.GetKeyDown(KeyCode.I) && inventoryRootPanel != null)
        {
            inventoryRootPanel.SetActive(!inventoryRootPanel.activeSelf);
        }
    }

    public void UpdatePage()
    {
        // ������ �ؽ�Ʈ ����
        pageText.text = $"{currentPage + 1}/{totalPages}";

        // ��/�� ȭ��ǥ ��ư�� interactable�� ���� (���� ���� X)
        var leftBtn = leftArrowImage.GetComponent<Button>();
        if (leftBtn != null)
            leftBtn.interactable = currentPage > 0;

        var rightBtn = rightArrowImage.GetComponent<Button>();
        if (rightBtn != null)
            rightBtn.interactable = currentPage < totalPages - 1;

        // ������ ���� ����
        for (int i = 0; i < itemSlotImages.Count; i++)
        {
            int itemIdx = currentPage * itemsPerPage + i;
            if (itemIdx < allItems.Count)
            {
                var item = allItems[itemIdx];
                bool owned = ownedItemIds.Contains(item.id);
                itemSlotImages[i].sprite = item.sprite; // �׻� ���� ��������Ʈ ���
                itemSlotImages[i].color = owned ? Color.white : Color.black; // ���� ���ο� ���� ��� ����

                // Ŭ�� �̺�Ʈ ���
                int idx = itemIdx; // Ŭ���� ���� ����
                var btn = itemSlotImages[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSlotClick(idx));
            }
            else
            {
                itemSlotImages[i].sprite = null; // �� ����
                itemSlotImages[i].color = new Color(1, 1, 1, 0); // ���� ó��
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
            // ������ ���� ǥ��
            chooseItemImage.sprite = item.sprite;
            chooseItemNameText.text = item.itemName;
            chooseItemDescriptionText.text = item.description;
        }
        else
        {
            // ��ȹ�� �ȳ� UI ǥ��
            if (notOwnedPanel != null)
            {
                notOwnedPanel.SetActive(true);
                StartCoroutine(HideNotOwnedPanel());
            }
        }
    }

    private System.Collections.IEnumerator HideNotOwnedPanel()
    {
        yield return new WaitForSeconds(2f); // 2�� ���
        if (notOwnedPanel != null)
            notOwnedPanel.SetActive(false); // �ȳ� UI ����
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