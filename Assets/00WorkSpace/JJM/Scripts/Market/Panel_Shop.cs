using System.Collections.Generic; 
using System.Linq; 
using TMPro; 
using UnityEngine; 
using NTJ;
using UnityEngine.UI;

public class Panel_Shop : MonoBehaviour // ���� UI �� ���� ���� Ŭ����
{
    public List<ItemData> sellItems; // �������� �Ǹ��� ������ ������ ����Ʈ (ScriptableObject)
    public Transform sellItemContent; // SellItem Scroll View�� Content Transform
    public Transform buyItemContent; // BuyItem Scroll View�� Content Transform
    public GameObject shopItemPrefab; // ������ �г� ������ (�̹���, �̸�, ����, ��ư ����)
    public TMP_Text buyCoinText; // ������ ������ ���� ������ ǥ���� �ؽ�Ʈ
    public TMP_Text coinText; // ���� �÷��̾ ���� ��ȭ�� ǥ���� �ؽ�Ʈ
    public Button btn_Esc;
    public Button btn_Buy;
    public int playerCoin = 99999; // �÷��̾ ���� ���� ��ȭ
    public GameObject notEnoughCoinPanel; // ��ȭ ���� �ȳ� UI ������Ʈ

    private List<ItemData> buyItems = new List<ItemData>(); // ���� ��Ͽ� �߰��� ������ ����Ʈ

    public HashSet<int> purchasedItemIds = new HashSet<int>(); // ������ ������ id ����

    
    public void Init() // ���� ���� �� ȣ��
    {
        UpdateCoinText(); // ���� ��ȭ �ؽ�Ʈ ����
        PopulateSellItems(); // �Ǹ� ������ ��� UI ����
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
        btn_Buy.onClick.AddListener(ConfirmBuy);
    }

    private void OnEnable()
    {
        
    }

    public void PopulateSellItems() // �Ǹ� ������ �г��� SellItemContent�� ����
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;
        // �ر� ���� üũ (���� ���� ��������)
        if (inventory != null) inventory.CheckUnlocks();

        // �������� ���� �������� ����, ������ �������� �Ʒ��� ������ ����
        var sortedItems = sellItems
            .Where(item => !buyItems.Contains(item)) // ���� ������ ���� �����۸� ǥ��
            .OrderBy(item => purchasedItemIds.Contains(item.id) ? 1 : 0)
            .ToList();
         
        foreach (Transform child in sellItemContent)
            Destroy(child.gameObject); // ���� �г� ��� ����

        foreach (var item in sortedItems) // ���ĵ� ����Ʈ ��ȸ
        {
            var go = Instantiate(shopItemPrefab, sellItemContent); // ������ ���� �� Content�� �߰�
            var ui = go.GetComponent<ItemUI>(); // ItemUI ������Ʈ ��������

            bool purchased = purchasedItemIds.Contains(item.id);

            // Ŭ�� �̺�Ʈ: �������� ���� �����۸� ���
            ui.Setup(item.sprite, item.itemName, item.price, item.description, purchased ? null : () => AddToBuyItems(item));

            // ��ư ������Ʈ ã�� (�ڽĿ� ����)
            var buttonTr = go.transform.Find("Button");
            if (buttonTr != null)
            {
                var btn = buttonTr.GetComponent<Button>();
                if (btn != null)
                    btn.interactable = !purchased;

                var btnImg = buttonTr.GetComponent<Image>();
                if (btnImg != null)
                    btnImg.color = purchased ? new Color(0.7f, 0.7f, 0.7f, 0.5f) : Color.black;
                // ���� ȸ�� + 50% ����
            }
        }
    }

    public void AddToBuyItems(ItemData item) // �Ǹ� ������ Ŭ�� �� ���� ��Ͽ� �߰�
    {
        if (buyItems.Any(x => x.id == item.id))// id ���� �ߺ� ����
            return; // �̹� ���� ��Ͽ� ������ �߰����� ����
        buyItems.Add(item); // ���� ��Ͽ� ������ �߰�
        var go = Instantiate(shopItemPrefab, buyItemContent); // BuyItemContent�� ������ ����
        var ui = go.GetComponent<ItemUI>(); // ItemUI ������Ʈ ��������
        ui.Setup(item.sprite, item.itemName, item.price, item.description, () => RemoveFromBuyItems(item, go)); // ������ ���� ����
        UpdateBuyTotal(); // ���� ���� ���� ����
        PopulateSellItems(); // ������ �������� ���ʿ��� ����
    }
    public void RemoveFromBuyItems(ItemData item, GameObject buyPanel)
    {
        buyItems.RemoveAll(x => x.id == item.id); // id ���� ����
        Destroy(buyPanel);
        UpdateBuyTotal();
        PopulateSellItems(); // ����� �������� �ٽ� ���ʿ� ǥ��
    }

    void UpdateBuyTotal() // ���� ����� �� ������ buyCoinText�� ǥ��
    {
        int total = buyItems.Sum(i => (int)i.price); // ���� ����� ���� �ջ�
        buyCoinText.text = total.ToString(); // ���� �ؽ�Ʈ ����
    }

    public void ConfirmBuy() // ���� ��ư Ŭ�� �� ȣ�� (�ϰ� ����)
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;

        int total = buyItems.Sum(i => (int)i.price); // ���� ����� �� ���� ���
        if (playerCoin >= total) // ��ȭ�� ������� Ȯ��
        {
            playerCoin -= total; // ��ȭ ����
            UpdateCoinText(); // ���� ��ȭ �ؽ�Ʈ ����

            // ������ ������ ������ ��� �� UI ����
            if (inventory != null)
            {
                foreach (var item in buyItems)
                    inventory.ownedItemIds.Add(item.id);

                inventory.CheckUnlocks();
                inventory.UpdatePage();
            }

            // 1. ������ ������ id�� ���տ� �߰�
            // ������ ������ id�� ���տ� �߰�
            foreach (var item in buyItems)
            {
                purchasedItemIds.Add(item.id);
            }

            // 2. ���� UI ���� (���� ������ �г� ��� ���� �� �ٽ� ����)
            

            buyItems.Clear(); // ���� ��� �ʱ�ȭ
            foreach (Transform child in buyItemContent) // ���� ��� UI ����
                Destroy(child.gameObject);
            buyCoinText.text = "0"; // ���� ���� �ؽ�Ʈ �ʱ�ȭ
                                    
            PopulateSellItems();// ���� ���� ���������� �� �� �� ����
        }
        else
        {
            Debug.Log("��ȭ ����! UI ǥ�� �õ�");
            // ��ȭ ���� �ȳ� UI ǥ��
            if (notEnoughCoinPanel != null)
            {
                notEnoughCoinPanel.SetActive(true);
                StartCoroutine(HideInsufficientCoinPanel());
            }
            else
            {
                Debug.LogWarning("notEnoughCoinPanel�� Inspector�� ����Ǿ� ���� �ʽ��ϴ�.");
            }
        }
    }
    
    // ��ȭ ���� �ȳ� UI�� 3�� �� �ڵ����� ���� �ڷ�ƾ
    private System.Collections.IEnumerator HideInsufficientCoinPanel()
    {
        yield return new WaitForSeconds(3f); // 3�� ���
        if (notEnoughCoinPanel != null)
            notEnoughCoinPanel.SetActive(false); // UI ����
    }

    void UpdateCoinText() // ���� ��ȭ �ؽ�Ʈ ����
    {
        coinText.text = playerCoin.ToString(); // coinText�� ���� ��ȭ ǥ��
    }
}