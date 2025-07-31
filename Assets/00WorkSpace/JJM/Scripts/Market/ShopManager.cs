using System.Collections.Generic; 
using System.Linq; 
using TMPro; 
using UnityEngine; 
using NTJ; 

public class ShopManager : MonoBehaviour // ���� UI �� ���� ���� Ŭ����
{
    public List<ItemData> sellItems; // �������� �Ǹ��� ������ ������ ����Ʈ (ScriptableObject)
    public Transform sellItemContent; // SellItem Scroll View�� Content Transform
    public Transform buyItemContent; // BuyItem Scroll View�� Content Transform
    public GameObject shopItemPrefab; // ������ �г� ������ (�̹���, �̸�, ����, ��ư ����)
    public TMP_Text buyCoinText; // ������ ������ ���� ������ ǥ���� �ؽ�Ʈ
    public TMP_Text coinText; // ���� �÷��̾ ���� ��ȭ�� ǥ���� �ؽ�Ʈ
    public int playerCoin = 999999; // �÷��̾ ���� ���� ��ȭ

    private List<ItemData> buyItems = new List<ItemData>(); // ���� ��Ͽ� �߰��� ������ ����Ʈ

    void Start() // ���� ���� �� ȣ��
    {
        UpdateCoinText(); // ���� ��ȭ �ؽ�Ʈ ����
        PopulateSellItems(); // �Ǹ� ������ ��� UI ����
    }

    void PopulateSellItems() // �Ǹ� ������ �г��� SellItemContent�� ����
    {
        foreach (var item in sellItems) // �Ǹ� ������ ����Ʈ ��ȸ
        {
            var go = Instantiate(shopItemPrefab, sellItemContent); // ������ ���� �� Content�� �߰�
            var ui = go.GetComponent<ItemUI>(); // ItemUI ������Ʈ ��������
            ui.Setup(item.sprite, item.itemName, item.value, () => AddToBuyItems(item)); // ������ ���� ���� �� Ŭ�� �̺�Ʈ ����
        }
    }

    public void AddToBuyItems(ItemData item) // �Ǹ� ������ Ŭ�� �� ���� ��Ͽ� �߰�
    {
        buyItems.Add(item); // ���� ��Ͽ� ������ �߰�
        var go = Instantiate(shopItemPrefab, buyItemContent); // BuyItemContent�� ������ ����
        var ui = go.GetComponent<ItemUI>(); // ItemUI ������Ʈ ��������
        ui.Setup(item.sprite, item.itemName, item.value, null); // ������ ���� ���� (���� ����� Ŭ�� �̺�Ʈ ����)
        UpdateBuyTotal(); // ���� ���� ���� ����
    }

    void UpdateBuyTotal() // ���� ����� �� ������ buyCoinText�� ǥ��
    {
        int total = buyItems.Sum(i => (int)i.value); // ���� ����� ���� �ջ�
        buyCoinText.text = total.ToString(); // ���� �ؽ�Ʈ ����
    }

    public void ConfirmBuy() // ���� ��ư Ŭ�� �� ȣ�� (�ϰ� ����)
    {
        int total = buyItems.Sum(i => (int)i.value); // ���� ����� �� ���� ���
        if (playerCoin >= total) // ��ȭ�� ������� Ȯ��
        {
            playerCoin -= total; // ��ȭ ����
            UpdateCoinText(); // ���� ��ȭ �ؽ�Ʈ ����
            buyItems.Clear(); // ���� ��� �ʱ�ȭ
            foreach (Transform child in buyItemContent) // ���� ��� UI ����
                Destroy(child.gameObject);
            buyCoinText.text = "0"; // ���� ���� �ؽ�Ʈ �ʱ�ȭ
        }
        else
        {
            // ��ȭ ���� �ȳ� (���� ����)
        }
    }

    void UpdateCoinText() // ���� ��ȭ �ؽ�Ʈ ����
    {
        coinText.text = playerCoin.ToString(); // coinText�� ���� ��ȭ ǥ��
    }
}