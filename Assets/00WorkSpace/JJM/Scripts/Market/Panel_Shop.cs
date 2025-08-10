using System.Collections.Generic; 
using System.Linq; 
using TMPro; 
using UnityEngine; 
using NTJ;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using System;

public class Panel_Shop : MonoBehaviour // 상점 UI 및 로직 관리 클래스
{
    public List<ItemData> sellItems; // 상점에서 판매할 아이템 데이터 리스트 (ScriptableObject)
    public Transform sellItemContent; // SellItem Scroll View의 Content Transform
    public Transform buyItemContent; // BuyItem Scroll View의 Content Transform
    public GameObject shopItemPrefab; // 아이템 패널 프리팹 (이미지, 이름, 가격, 버튼 포함)
    public TMP_Text buyCoinText; // 구매할 아이템 총합 가격을 표시할 텍스트
    public TMP_Text coinText; // 현재 플레이어가 가진 재화를 표시할 텍스트
    public Button btn_Esc;
    public Button btn_Buy;
    public int playerCoin; // 플레이어가 가진 현재 재화
    public GameObject notEnoughCoinPanel; // 재화 부족 안내 UI 오브젝트

    private List<ItemData> buyItems = new List<ItemData>(); // 구매 목록에 추가된 아이템 리스트

    //public HashSet<int> purchasedItemIds = new HashSet<int>(); // 구매한 아이템 id 집합

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Init() // 게임 시작 시 호출
    {
        PopulateSellItems(); // 판매 아이템 목록 UI 생성
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
        btn_Buy.onClick.AddListener(ConfirmBuy);
    }

    private void OnEnable()
    {
        UpdateShopSlotsView();
        
        UpdateCoinText();
    }

    public void PopulateSellItems() // 판매 아이템 패널을 SellItemContent에 생성
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;

        List<int> purchasedItemIds = new List<int>();
        int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
        if (owneditemsArray != null)
            purchasedItemIds = owneditemsArray.ToList();

        // 해금 조건 체크 (상점 갱신 직전마다)
        if (inventory != null) inventory.CheckUnlocks();

        // 구매하지 않은 아이템이 먼저, 구매한 아이템이 아래로 오도록 정렬
        var sortedItems = sellItems
            .Where(item => !buyItems.Contains(item)) // 구매 예정에 없는 아이템만 표시
            .OrderBy(item => purchasedItemIds.Contains(item.id) ? 1 : 0)
            .ToList();
         
        foreach (Transform child in sellItemContent)
            Destroy(child.gameObject); // 기존 패널 모두 삭제

        foreach (var item in sortedItems) // 정렬된 리스트 순회
        {
            var go = Instantiate(shopItemPrefab, sellItemContent); // 프리팹 생성 및 Content에 추가
            var ui = go.GetComponent<ItemUI>(); // ItemUI 컴포넌트 가져오기

            bool purchased = purchasedItemIds.Contains(item.id);

            // 클릭 이벤트: 구매하지 않은 아이템만 등록
            ui.Setup(item.sprite, item.itemName, item.price, item.description, purchased ? null : () => AddToBuyItems(item));

            // 버튼 오브젝트 찾기 (자식에 있음)
            var buttonTr = go.transform.Find("Button");
            if (buttonTr != null)
            {
                var btn = buttonTr.GetComponent<Button>();
                if (btn != null)
                    btn.interactable = !purchased;

                var btnImg = buttonTr.GetComponent<Image>();
                if (btnImg != null)
                    btnImg.color = purchased ? new Color(0.7f, 0.7f, 0.7f, 0.5f) : Color.black;
                // 밝은 회색 + 50% 투명
            }
        }
    }

    public void AddToBuyItems(ItemData item) // 판매 아이템 클릭 시 구매 목록에 추가
    {
        if (buyItems.Any(x => x.id == item.id))// id 기준 중복 방지
            return; // 이미 구매 목록에 있으면 추가하지 않음
        buyItems.Add(item); // 구매 목록에 아이템 추가
        var go = Instantiate(shopItemPrefab, buyItemContent); // BuyItemContent에 프리팹 생성
        var ui = go.GetComponent<ItemUI>(); // ItemUI 컴포넌트 가져오기
        ui.Setup(item.sprite, item.itemName, item.price, item.description, () => RemoveFromBuyItems(item, go)); // 아이템 정보 세팅
        UpdateBuyTotal(); // 구매 총합 가격 갱신
        PopulateSellItems(); // 선택한 아이템은 왼쪽에서 제외
    }
    public void RemoveFromBuyItems(ItemData item, GameObject buyPanel)
    {
        buyItems.RemoveAll(x => x.id == item.id); // id 기준 삭제
        Destroy(buyPanel);
        UpdateBuyTotal();
        PopulateSellItems(); // 취소한 아이템을 다시 왼쪽에 표시
    }

    void UpdateBuyTotal() // 구매 목록의 총 가격을 buyCoinText에 표시
    {
        int total = buyItems.Sum(i => (int)i.price); // 구매 목록의 가격 합산
        buyCoinText.text = total.ToString(); // 총합 텍스트 갱신
    }


    // 이거 좋은듯?
    IEnumerator WaitUntilCustomPropertyChanged(string key, Action action, object preValue)
    {
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties[key] != preValue);
        action?.Invoke();
    }

    public void ConfirmBuy() // 구매 버튼 클릭 시 호출 (일괄 구매)
    {
        Panel_Inventory inventory = UIManager.Instance.LobbyGroup.panel_Inventory;

        int total = buyItems.Sum(i => (int)i.price); // 구매 목록의 총 가격 계산
        if (playerCoin >= total) // 재화가 충분한지 확인
        {
            // 로그인 유저라면 DB까지 돈 차감
            if (BackendManager.Auth.CurrentUser != null)
            {
                BackendManager.Instance.LoadUserDataFromDB((userData) =>
                {
                    UserData updatedUserData = userData;
                    updatedUserData.money -= total;

                    object preValue = PhotonNetwork.LocalPlayer.CustomProperties["Money"];

                    // 서버에 유저 데이터를 갱신
                    BackendManager.Instance.InitUserDataToDB(updatedUserData, () =>
                    {
                        // 갱신 완료 시 서버의 유저데이터를 클라이언트에 동기화(커스텀 프로퍼티에도 이 함수에서 적용 됨)
                        NetworkManager.Instance.UpdateUserDataToClient(userData);

                        // 커스텀 프로퍼티 설정될 때까지 기다렸다가, 현재 재화 텍스트 갱신
                        StartCoroutine(WaitUntilCustomPropertyChanged("Money", () => UpdateCoinText(), preValue));
                    });
                });
            }
            // 게스트 유저면 커스텀 프로퍼티에만 적용
            else
            {
                object preValue = PhotonNetwork.LocalPlayer.CustomProperties["Money"];
                PhotonNetwork.LocalPlayer.CustomProperties["Money"] = (int)preValue - total;

                // 커스텀 프로퍼티 설정될 때까지 기다렸다가, 현재 재화 텍스트 갱신
                StartCoroutine(WaitUntilCustomPropertyChanged("Money", () => UpdateCoinText(), preValue));
            }


            // 도감에 구매한 아이템 등록 및 UI 갱신
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

                // 커스텀 프로퍼티에 저장
                ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
                playerProperty["OwnedItems"] = ownedItemIds.ToArray();
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

                // 로그인 유저는 DB까지 저장
                if (BackendManager.Auth.CurrentUser != null)
                    BackendManager.Instance.UpdateUserDataValue("owndItemList", ownedItemIds);

                inventory.CheckUnlocks();
                inventory.UpdatePage();
            }

            // 상점 UI 갱신 (기존 아이템 패널 모두 삭제 후 다시 생성)
            UpdateShopSlotsView();
        }
        else
        {
            Debug.Log("재화 부족! UI 표시 시도");
            // 재화 부족 안내 UI 표시
            if (notEnoughCoinPanel != null)
            {
                notEnoughCoinPanel.SetActive(true);
                StartCoroutine(HideInsufficientCoinPanel());
            }
            else
            {
                Debug.LogWarning("notEnoughCoinPanel이 Inspector에 연결되어 있지 않습니다.");
            }
        }
    }

    // 상점 UI 갱신 (기존 아이템 패널 모두 삭제 후 다시 생성)
    void UpdateShopSlotsView()
    {
        buyItems.Clear(); // 구매 목록 초기화
        foreach (Transform child in buyItemContent) // 구매 목록 UI 삭제
            Destroy(child.gameObject);
        buyCoinText.text = "0"; // 구매 총합 텍스트 초기화

        PopulateSellItems();// 구매 직후 상점정보를 한 번 더 갱신
    }

    // 재화 부족 안내 UI를 3초 후 자동으로 끄는 코루틴
    private System.Collections.IEnumerator HideInsufficientCoinPanel()
    {
        yield return new WaitForSeconds(3f); // 3초 대기
        if (notEnoughCoinPanel != null)
            notEnoughCoinPanel.SetActive(false); // UI 끄기
    }

    void UpdateCoinText() // 현재 재화 텍스트 갱신
    {
        int curMoney = (int)PhotonNetwork.LocalPlayer.CustomProperties["Money"];
        playerCoin = curMoney;
        coinText.text = playerCoin.ToString(); // coinText에 현재 재화 표시
    }
}