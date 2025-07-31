using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<ShopItem> sellItems; // ������ ��ϵ� ������ ���
    public Transform sellItemContent; // SellItem Scroll View Content
    public Transform buyItemContent;  // BuyItem Scroll View Content
    public GameObject shopItemPrefab; // ������ ������(�̹���, �̸�, ��, ���Ź�ư ����)
    public TMP_Text buyCoinText;
    public int playerCoin = 999999;

    private List<ShopItem> buyItems = new List<ShopItem>();

    void Start()
    {
        UpdateCoinText();
        PopulateSellItems();
    }

    void PopulateSellItems()
    {
        foreach (var item in sellItems)
        {
            var go = Instantiate(shopItemPrefab, sellItemContent);
            // �������� �̹���, �̸�, �� ����
            // ���� ��ư�� OnClick �̺�Ʈ ����: () => AddToBuyItems(item)
        }
    }

    public void AddToBuyItems(ShopItem item)
    {
        buyItems.Add(item);
        var go = Instantiate(shopItemPrefab, buyItemContent);
        // �������� �̹���, �̸�, �� ���� (���� ��ư�� ��Ȱ��ȭ)
        UpdateBuyTotal();
    }

    void UpdateBuyTotal()
    {
        int total = buyItems.Sum(i => i.itemValue);
        // BuyItem Scroll View �Ʒ��� ���� ǥ�� ����
    }

    public void ConfirmBuy()
    {
        int total = buyItems.Sum(i => i.itemValue);
        if (playerCoin >= total)
        {
            playerCoin -= total;
            UpdateCoinText();
            buyItems.Clear();
            // BuyItem Content ����
        }
        else
        {
            // ��ȭ ���� �ȳ�
        }
    }

    void UpdateCoinText()
    {
        buyCoinText.text = playerCoin.ToString();
    }
}
