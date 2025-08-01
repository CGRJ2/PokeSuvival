using System.Collections.Generic; 
using System.Linq; 
using TMPro; 
using UnityEngine; 
using NTJ; 

public class ShopManager : MonoBehaviour // 상점 UI 및 로직 관리 클래스
{
    public List<ItemData> sellItems; // 상점에서 판매할 아이템 데이터 리스트 (ScriptableObject)
    public Transform sellItemContent; // SellItem Scroll View의 Content Transform
    public Transform buyItemContent; // BuyItem Scroll View의 Content Transform
    public GameObject shopItemPrefab; // 아이템 패널 프리팹 (이미지, 이름, 가격, 버튼 포함)
    public TMP_Text buyCoinText; // 구매할 아이템 총합 가격을 표시할 텍스트
    public TMP_Text coinText; // 현재 플레이어가 가진 재화를 표시할 텍스트
    public int playerCoin = 99999; // 플레이어가 가진 현재 재화
    public GameObject notEnoughCoinPanel; // 재화 부족 안내 UI 오브젝트
    public GameObject shopRootPanel; // 상점 전체 오브젝트

    private List<ItemData> buyItems = new List<ItemData>(); // 구매 목록에 추가된 아이템 리스트

    void Start() // 게임 시작 시 호출
    {
        UpdateCoinText(); // 현재 재화 텍스트 갱신
        PopulateSellItems(); // 판매 아이템 목록 UI 생성
    }

    void PopulateSellItems() // 판매 아이템 패널을 SellItemContent에 생성
    {
        foreach (var item in sellItems) // 판매 아이템 리스트 순회
        {
            var go = Instantiate(shopItemPrefab, sellItemContent); // 프리팹 생성 및 Content에 추가
            var ui = go.GetComponent<ItemUI>(); // ItemUI 컴포넌트 가져오기
            ui.Setup(item.sprite, item.itemName, item.value, () => AddToBuyItems(item)); // 아이템 정보 세팅 및 클릭 이벤트 연결
        }
    }

    public void AddToBuyItems(ItemData item) // 판매 아이템 클릭 시 구매 목록에 추가
    {
        buyItems.Add(item); // 구매 목록에 아이템 추가
        var go = Instantiate(shopItemPrefab, buyItemContent); // BuyItemContent에 프리팹 생성
        var ui = go.GetComponent<ItemUI>(); // ItemUI 컴포넌트 가져오기
        ui.Setup(item.sprite, item.itemName, item.value, null); // 아이템 정보 세팅 (구매 목록은 클릭 이벤트 없음)
        UpdateBuyTotal(); // 구매 총합 가격 갱신
    }

    void UpdateBuyTotal() // 구매 목록의 총 가격을 buyCoinText에 표시
    {
        int total = buyItems.Sum(i => (int)i.value); // 구매 목록의 가격 합산
        buyCoinText.text = total.ToString(); // 총합 텍스트 갱신
    }

    public void ConfirmBuy() // 구매 버튼 클릭 시 호출 (일괄 구매)
    {
        int total = buyItems.Sum(i => (int)i.value); // 구매 목록의 총 가격 계산
        if (playerCoin >= total) // 재화가 충분한지 확인
        {
            playerCoin -= total; // 재화 차감
            UpdateCoinText(); // 현재 재화 텍스트 갱신
            buyItems.Clear(); // 구매 목록 초기화
            foreach (Transform child in buyItemContent) // 구매 목록 UI 삭제
                Destroy(child.gameObject);
            buyCoinText.text = "0"; // 구매 총합 텍스트 초기화
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
    
    // 재화 부족 안내 UI를 3초 후 자동으로 끄는 코루틴
    private System.Collections.IEnumerator HideInsufficientCoinPanel()
    {
        yield return new WaitForSeconds(3f); // 3초 대기
        if (notEnoughCoinPanel != null)
            notEnoughCoinPanel.SetActive(false); // UI 끄기
    }

    public void CloseShop()//상점 닫기 버튼 클릭 시 호출
    {
        if (shopRootPanel != null)
            shopRootPanel.SetActive(false); // 상점 UI 비활성화
    }
    void UpdateCoinText() // 현재 재화 텍스트 갱신
    {
        coinText.text = playerCoin.ToString(); // coinText에 현재 재화 표시
    }
}