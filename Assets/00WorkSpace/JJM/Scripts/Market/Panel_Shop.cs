using System.Collections.Generic; 
using System.Linq; 
using TMPro; 
using UnityEngine; 
using NTJ;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using System;

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
    public int playerCoin; // �÷��̾ ���� ���� ��ȭ
    public GameObject notEnoughCoinPanel; // ��ȭ ���� �ȳ� UI ������Ʈ

    private List<ItemData> buyItems = new List<ItemData>(); // ���� ��Ͽ� �߰��� ������ ����Ʈ

    //public HashSet<int> purchasedItemIds = new HashSet<int>(); // ������ ������ id ����

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Init() // ���� ���� �� ȣ��
    {
        PopulateSellItems(); // �Ǹ� ������ ��� UI ����
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
        btn_Buy.onClick.AddListener(ConfirmBuy);
    }

    private void OnEnable()
    {
        UpdateShopSlotsView();
        
        UpdateCoinText();
    }

    public void PopulateSellItems() // �Ǹ� ������ �г��� SellItemContent�� ����
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;

        List<int> purchasedItemIds = new List<int>();
        int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
        if (owneditemsArray != null)
            purchasedItemIds = owneditemsArray.ToList();

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


    // �̰� ������?
    IEnumerator WaitUntilCustomPropertyChanged(string key, Action action, object preValue)
    {
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties[key] != preValue);
        action?.Invoke();
    }

    public void ConfirmBuy() // ���� ��ư Ŭ�� �� ȣ�� (�ϰ� ����)
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;

        int total = buyItems.Sum(i => (int)i.price); // ���� ����� �� ���� ���
        if (playerCoin >= total) // ��ȭ�� ������� Ȯ��
        {
            // �α��� ������� DB���� �� ����
            if (BackendManager.Auth.CurrentUser != null)
            {
                BackendManager.Instance.LoadUserDataFromDB((userData) =>
                {
                    UserData updatedUserData = userData;
                    updatedUserData.money -= total;

                    object preValue = PhotonNetwork.LocalPlayer.CustomProperties["Money"];

                    // ������ ���� �����͸� ����
                    BackendManager.Instance.InitUserDataToDB(updatedUserData, () =>
                    {
                        // ���� �Ϸ� �� ������ ���������͸� Ŭ���̾�Ʈ�� ����ȭ(Ŀ���� ������Ƽ���� �� �Լ����� ���� ��)
                        NetworkManager.Instance.UpdateUserDataToClient(userData);

                        // Ŀ���� ������Ƽ ������ ������ ��ٷȴٰ�, ���� ��ȭ �ؽ�Ʈ ����
                        StartCoroutine(WaitUntilCustomPropertyChanged("Money", () => UpdateCoinText(), preValue));
                    });
                });
            }
            // �Խ�Ʈ ������ Ŀ���� ������Ƽ���� ����
            else
            {
                object preValue = PhotonNetwork.LocalPlayer.CustomProperties["Money"];
                PhotonNetwork.LocalPlayer.CustomProperties["Money"] = (int)preValue - total;

                // Ŀ���� ������Ƽ ������ ������ ��ٷȴٰ�, ���� ��ȭ �ؽ�Ʈ ����
                StartCoroutine(WaitUntilCustomPropertyChanged("Money", () => UpdateCoinText(), preValue));
            }


            // ������ ������ ������ ��� �� UI ����
            if (inventory != null)
            {
                List<int> ownedItemIds = new List<int>();
                int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
                if (owneditemsArray != null)
                    ownedItemIds = owneditemsArray.ToList();

                foreach (var item in buyItems)
                {
                    ownedItemIds.Add(item.id);
                }

                // Ŀ���� ������Ƽ�� ����
                ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
                playerProperty["OwnedItems"] = ownedItemIds.ToArray();
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

                // �α��� ������ DB���� ����
                if (BackendManager.Auth.CurrentUser != null)
                    BackendManager.Instance.UpdateUserDataValue("owndItemList", ownedItemIds);

                inventory.CheckUnlocks();
                inventory.UpdatePage();
            }

            // ���� UI ���� (���� ������ �г� ��� ���� �� �ٽ� ����)
            UpdateShopSlotsView();
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

    // ���� UI ���� (���� ������ �г� ��� ���� �� �ٽ� ����)
    void UpdateShopSlotsView()
    {
        buyItems.Clear(); // ���� ��� �ʱ�ȭ
        foreach (Transform child in buyItemContent) // ���� ��� UI ����
            Destroy(child.gameObject);
        buyCoinText.text = "0"; // ���� ���� �ؽ�Ʈ �ʱ�ȭ

        PopulateSellItems();// ���� ���� ���������� �� �� �� ����
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
        int curMoney = (int)PhotonNetwork.LocalPlayer.CustomProperties["Money"];
        playerCoin = curMoney;
        coinText.text = playerCoin.ToString(); // coinText�� ���� ��ȭ ǥ��
    }
}