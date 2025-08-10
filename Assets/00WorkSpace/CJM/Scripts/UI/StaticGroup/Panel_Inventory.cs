using NTJ;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Panel_Inventory : MonoBehaviour
{
    [Header("Ÿ��Ʋ �� ���� ������ �г�")]
    [SerializeField] TMP_Text tmp_Title; // Title Panel�� Title Text (TMP)
    [SerializeField] Image image_SelectedItem; // ���õ� �������� �̹���
    [SerializeField] TMP_Text tmp_SelectedItemName; // ���õ� �������� �̸�
    [SerializeField] TMP_Text tmp_SelectedItemDescription; // ���õ� �������� ����
    

    [SerializeField] Transform itemSlotsParent;
    public Button btn_Esc;


    Slot_Inventory[] slots; // ������ ����Ʈ �г��� �� ������ �̹��� ����

    [Header("���������̼�")]
    public Button btn_Left; // ���� ȭ��ǥ �̹���
    public Button btn_Right; // ������ ȭ��ǥ �̹���
    public TMP_Text pageText; // ���� ������/��ü ������ ǥ��

    public int itemsPerPage = 6; // �� �������� ������ ������ ����
    private int currentPage = 0; // ���� ������(0���� ����)
    private int totalPages = 1; // ��ü ������ ��

    [Header("������")]
    public List<ItemData> allItems = new List<ItemData>(); // ��ü ������ ������ ����Ʈ
    //public HashSet<int> ownedItemIds = new HashSet<int>(); // ������ ������ id ����

    [Header("��ȹ�� �ȳ� UI")]
    public GameObject notOwnedPanel; // "���� ȹ������ �ʾҽ��ϴ�" �ȳ� UI



    private HashSet<int> unlockedItemIds = new HashSet<int>(); // �رݵ� ������ id ����
    private Dictionary<int, bool> unlockConditions = new Dictionary<int, bool>();


    public void Init()
    {
        slots = itemSlotsParent.GetComponentsInChildren<Slot_Inventory>();
        allItems.AddRange(UIManager.Instance.LobbyGroup.panel_Shop.sellItems);
        allItems.Add(Resources.Load<ItemData>("SellItem/AmuletCoin"));
        RegisterUnlockConditions(); // �ر� ���� ���!
        totalPages = Mathf.CeilToInt((float)allItems.Count / itemsPerPage); // ��ü ������ ���
        CheckUnlocks();
        UpdatePage(); // ù ������ ǥ��

        btn_Left.onClick.AddListener(OnLeftArrowClick);
        btn_Right.onClick.AddListener(OnRightArrowClick);
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void OnEnable()
    {
        UpdatePage();
        int equipedItemKey = (int)PhotonNetwork.LocalPlayer.CustomProperties["HeldItem"];
        Debug.Log($"���� ���� ���� {Define.GetItemById(equipedItemKey)}");
        if (PhotonNetwork.LocalPlayer.CustomProperties["HeldItem"] == null)
            UpdateSelectedItemInfoView(null);
        else 
            UpdateSelectedItemInfoView(Define.GetItemById(equipedItemKey));
    }

    // �ر� ���� ��� ����
    void RegisterUnlockConditions()
    {
        Panel_Shop shop = UIManager.Instance.LobbyGroup.panel_Shop;

        // ����: id�� 10006�� �������� ��� ���� ������ ���� �� �ر�
        List<int> purchasedItemIds = new List<int>();
        int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
        if (owneditemsArray != null)
            purchasedItemIds = owneditemsArray.ToList();

        unlockConditions[10006] = shop.sellItems.All(item => purchasedItemIds.Contains(item.id));
        // ���� ������ ShopManager���� public���� expose �ʿ�
        // ���� ������ �߰��� �� ����
    }

    // �ر� üũ
    public void CheckUnlocks()
    {
        Panel_Shop shop = UIManager.Instance.LobbyGroup.panel_Shop;


        foreach (var kvp in unlockConditions)
        {
            bool conditionMet = kvp.Value;
            Debug.Log($"�ر� ���� üũ: {kvp.Key}, ���� ���: {conditionMet}");

            if (!unlockedItemIds.Contains(kvp.Key) && conditionMet)
            {
                unlockedItemIds.Add(kvp.Key);

                Debug.Log($"������ �ر�: {kvp.Key}");

                // �رݵ� �������� ������ �߰�
                var unlockedItem = allItems.FirstOrDefault(x => x.id == kvp.Key);
                if (unlockedItem != null && !shop.sellItems.Contains(unlockedItem))
                {
                    shop.sellItems.Add(unlockedItem);
                    shop.PopulateSellItems(); // ���� UI ����
                }
                // ����(�κ��丮) UI�� ��� ����
                UpdatePage();
            }
        }
    }

    public void UpdatePage()
    {
        // ������ �ؽ�Ʈ ����
        pageText.text = $"{currentPage + 1}/{totalPages}";

        // ��/�� ȭ��ǥ ��ư�� interactable���� 
        btn_Left.interactable = currentPage > 0;

        btn_Right.interactable = currentPage < totalPages - 1;

        // ������ ���� ����
        for (int i = 0; i < slots.Length; i++)
        {
            int itemIdx = currentPage * itemsPerPage + i;
            if (itemIdx < allItems.Count)
            {
                var item = allItems[itemIdx];

                int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
                List<int> ownedItemIds = new List<int>();
                if (owneditemsArray != null)
                    ownedItemIds = owneditemsArray.ToList();

                bool owned = ownedItemIds.Contains(item.id);
                // �ر� ������ ��ϵ� �����۸� �ر� üũ, �׷��� ������ �׻� �رݵ� ������ ó��

                bool isUnlockItem = unlockConditions.ContainsKey(item.id);
                bool unlocked = isUnlockItem ? unlockedItemIds.Contains(item.id) : true;


                if (isUnlockItem)
                {
                    // �ر� ������
                    if (!unlocked)
                    {
                        // �ر� ��: ������
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.black;
                    }
                    else if (!owned)
                    {
                        // �ر� ��, �̱���: ����/ȸ��
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    else
                    {
                        // �ر� ��, ����: ���� ��������Ʈ(���)
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.white;
                    }
                }
                else
                {
                    // ���� �Ǹ� ������
                    if (!owned)
                    {
                        // ���� ��: ������
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.black;
                    }
                    else
                    {
                        // ���� ��: ���� ��������Ʈ(���)
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.white;
                    }
                }


                //itemSlotImages[i].color = owned ? Color.white : Color.black; // ���� ���ο� ���� ��� ����

                // Ŭ�� �̺�Ʈ ���
                int idx = itemIdx; // Ŭ���� ���� ����
                var btn = slots[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSlotClick(idx));
            }
            else
            {
                slots[i].image_Item.sprite = null; // �� ����
                slots[i].image_Item.color = new Color(1, 1, 1, 0); // ���� ó��
                var btn = slots[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    public void OnItemSlotClick(int itemIdx)
    {
        var item = allItems[itemIdx];

        int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
        List<int> ownedItemIds = new List<int>();
        if (owneditemsArray != null)
            ownedItemIds = owneditemsArray.ToList();

        bool owned = ownedItemIds.Contains(item.id);
        bool isUnlockItem = unlockConditions.ContainsKey(item.id);
        bool unlocked = unlockedItemIds.Contains(item.id);
        if (isUnlockItem && !unlocked)
        {
            // �ر� ��: �ƹ� ������ ǥ������ ����
            if (notOwnedPanel != null)
            {
                notOwnedPanel.SetActive(true);
                StartCoroutine(HideNotOwnedPanel());
            }
            return;
        }
        if (owned)
        {
            // ������ ���� ǥ��
            UpdateSelectedItemInfoView(item);

            // ����
            // ���⼭ ���������� ������Ƽ ����

            // Ŀ���� ������Ƽ�� ����
            ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
            playerProperty["HeldItem"] = item.id;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

            // �α��� ������� DB�� ���������� ���� ������Ʈ
            if (BackendManager.Auth.CurrentUser != null)
            {
                BackendManager.Instance.UpdateUserDataValue("heldItem", item.id);
            }
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

    private void UpdateSelectedItemInfoView(ItemData item)
    {
        if (item == null)
        {
            image_SelectedItem.gameObject.SetActive(false);
            image_SelectedItem.sprite = null;
            tmp_SelectedItemName.text = "";
            tmp_SelectedItemDescription.text = "";
        }
        else
        {
            image_SelectedItem.gameObject.SetActive(true);
            image_SelectedItem.sprite = item.sprite;
            tmp_SelectedItemName.text = item.itemName;
            tmp_SelectedItemDescription.text = item.description;
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
