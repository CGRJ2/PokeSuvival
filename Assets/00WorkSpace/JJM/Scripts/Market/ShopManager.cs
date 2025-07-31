using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<ShopItem> sellItems; // 상점에 등록된 아이템 목록
    public Transform sellItemContent; // SellItem Scroll View Content
    public Transform buyItemContent;  // BuyItem Scroll View Content
    public GameObject shopItemPrefab; // 아이템 프리팹(이미지, 이름, 값, 구매버튼 포함)
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
            // 프리팹의 이미지, 이름, 값 설정
            // 구매 버튼에 OnClick 이벤트 연결: () => AddToBuyItems(item)
        }
    }

    public void AddToBuyItems(ShopItem item)
    {
        buyItems.Add(item);
        var go = Instantiate(shopItemPrefab, buyItemContent);
        // 프리팹의 이미지, 이름, 값 설정 (구매 버튼은 비활성화)
        UpdateBuyTotal();
    }

    void UpdateBuyTotal()
    {
        int total = buyItems.Sum(i => i.itemValue);
        // BuyItem Scroll View 아래에 총합 표시 가능
    }

    public void ConfirmBuy()
    {
        int total = buyItems.Sum(i => i.itemValue);
        if (playerCoin >= total)
        {
            playerCoin -= total;
            UpdateCoinText();
            buyItems.Clear();
            // BuyItem Content 비우기
        }
        else
        {
            // 재화 부족 안내
        }
    }

    void UpdateCoinText()
    {
        buyCoinText.text = playerCoin.ToString();
    }
}
