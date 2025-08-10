using NTJ;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Panel_Inventory : MonoBehaviour
{
    [Header("타이틀 및 선택 아이템 패널")]
    [SerializeField] TMP_Text tmp_Title; // Title Panel의 Title Text (TMP)
    [SerializeField] Image image_SelectedItem; // 선택된 아이템의 이미지
    [SerializeField] TMP_Text tmp_SelectedItemName; // 선택된 아이템의 이름
    [SerializeField] TMP_Text tmp_SelectedItemDescription; // 선택된 아이템의 설명
    

    [SerializeField] Transform itemSlotsParent;
    public Button btn_Esc;


    Slot_Inventory[] slots; // 아이템 리스트 패널의 각 아이템 이미지 슬롯

    [Header("페이지네이션")]
    public Button btn_Left; // 왼쪽 화살표 이미지
    public Button btn_Right; // 오른쪽 화살표 이미지
    public TMP_Text pageText; // 현재 페이지/전체 페이지 표시

    public int itemsPerPage = 6; // 한 페이지에 보여줄 아이템 개수
    private int currentPage = 0; // 현재 페이지(0부터 시작)
    private int totalPages = 1; // 전체 페이지 수

    [Header("데이터")]
    public List<ItemData> allItems = new List<ItemData>(); // 전체 아이템 데이터 리스트
    //public HashSet<int> ownedItemIds = new HashSet<int>(); // 구매한 아이템 id 집합

    [Header("미획득 안내 UI")]
    public GameObject notOwnedPanel; // "아직 획득하지 않았습니다" 안내 UI



    private HashSet<int> unlockedItemIds = new HashSet<int>(); // 해금된 아이템 id 집합
    private Dictionary<int, bool> unlockConditions = new Dictionary<int, bool>();


    public void Init()
    {
        slots = itemSlotsParent.GetComponentsInChildren<Slot_Inventory>();
        allItems.AddRange(UIManager.Instance.LobbyGroup.panel_Shop.sellItems);
        allItems.Add(Resources.Load<ItemData>("SellItem/AmuletCoin"));
        RegisterUnlockConditions(); // 해금 조건 등록!
        totalPages = Mathf.CeilToInt((float)allItems.Count / itemsPerPage); // 전체 페이지 계산
        CheckUnlocks();
        UpdatePage(); // 첫 페이지 표시

        btn_Left.onClick.AddListener(OnLeftArrowClick);
        btn_Right.onClick.AddListener(OnRightArrowClick);
        btn_Esc.onClick.AddListener(() => UIManager.Instance.ClosePanel(gameObject));
    }

    private void OnEnable()
    {
        UpdatePage();
        int equipedItemKey = (int)PhotonNetwork.LocalPlayer.CustomProperties["HeldItem"];
        Debug.Log($"현재 지닌 물건 {Define.GetItemById(equipedItemKey)}");
        if (PhotonNetwork.LocalPlayer.CustomProperties["HeldItem"] == null)
            UpdateSelectedItemInfoView(null);
        else 
            UpdateSelectedItemInfoView(Define.GetItemById(equipedItemKey));
    }

    // 해금 조건 등록 예시
    void RegisterUnlockConditions()
    {
        Panel_Shop shop = UIManager.Instance.LobbyGroup.panel_Shop;

        // 예시: id가 10006인 아이템은 모든 상점 아이템 구매 시 해금
        List<int> purchasedItemIds = new List<int>();
        int[] owneditemsArray = (int[])PhotonNetwork.LocalPlayer.CustomProperties["OwnedItems"];
        if (owneditemsArray != null)
            purchasedItemIds = owneditemsArray.ToList();

        unlockConditions[10006] = shop.sellItems.All(item => purchasedItemIds.Contains(item.id));
        // 실제 조건은 ShopManager에서 public으로 expose 필요
        // 여러 조건을 추가할 수 있음
    }

    // 해금 체크
    public void CheckUnlocks()
    {
        Panel_Shop shop = UIManager.Instance.LobbyGroup.panel_Shop;


        foreach (var kvp in unlockConditions)
        {
            bool conditionMet = kvp.Value;
            Debug.Log($"해금 조건 체크: {kvp.Key}, 조건 결과: {conditionMet}");

            if (!unlockedItemIds.Contains(kvp.Key) && conditionMet)
            {
                unlockedItemIds.Add(kvp.Key);

                Debug.Log($"아이템 해금: {kvp.Key}");

                // 해금된 아이템을 상점에 추가
                var unlockedItem = allItems.FirstOrDefault(x => x.id == kvp.Key);
                if (unlockedItem != null && !shop.sellItems.Contains(unlockedItem))
                {
                    shop.sellItems.Add(unlockedItem);
                    shop.PopulateSellItems(); // 상점 UI 갱신
                }
                // 도감(인벤토리) UI도 즉시 갱신
                UpdatePage();
            }
        }
    }

    public void UpdatePage()
    {
        // 페이지 텍스트 갱신
        pageText.text = $"{currentPage + 1}/{totalPages}";

        // 좌/우 화살표 버튼의 interactable조절 
        btn_Left.interactable = currentPage > 0;

        btn_Right.interactable = currentPage < totalPages - 1;

        // 아이템 슬롯 갱신
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
                // 해금 조건이 등록된 아이템만 해금 체크, 그렇지 않으면 항상 해금된 것으로 처리

                bool isUnlockItem = unlockConditions.ContainsKey(item.id);
                bool unlocked = isUnlockItem ? unlockedItemIds.Contains(item.id) : true;


                if (isUnlockItem)
                {
                    // 해금 아이템
                    if (!unlocked)
                    {
                        // 해금 전: 검은색
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.black;
                    }
                    else if (!owned)
                    {
                        // 해금 후, 미구매: 투명/회색
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    else
                    {
                        // 해금 후, 구매: 원래 스프라이트(흰색)
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.white;
                    }
                }
                else
                {
                    // 상점 판매 아이템
                    if (!owned)
                    {
                        // 구매 전: 검은색
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.black;
                    }
                    else
                    {
                        // 구매 후: 원래 스프라이트(흰색)
                        slots[i].image_Item.sprite = item.sprite;
                        slots[i].image_Item.color = Color.white;
                    }
                }


                //itemSlotImages[i].color = owned ? Color.white : Color.black; // 소유 여부에 따라 밝기 변경

                // 클릭 이벤트 등록
                int idx = itemIdx; // 클로저 문제 방지
                var btn = slots[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSlotClick(idx));
            }
            else
            {
                slots[i].image_Item.sprite = null; // 빈 슬롯
                slots[i].image_Item.color = new Color(1, 1, 1, 0); // 투명 처리
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
            // 해금 전: 아무 정보도 표시하지 않음
            if (notOwnedPanel != null)
            {
                notOwnedPanel.SetActive(true);
                StartCoroutine(HideNotOwnedPanel());
            }
            return;
        }
        if (owned)
        {
            // 아이템 정보 표시
            UpdateSelectedItemInfoView(item);

            // 선택
            // 여기서 장착아이템 프로퍼티 설정

            // 커스텀 프로퍼티에 저장
            ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
            playerProperty["HeldItem"] = item.id;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);

            // 로그인 유저라면 DB에 장착아이템 정보 업데이트
            if (BackendManager.Auth.CurrentUser != null)
            {
                BackendManager.Instance.UpdateUserDataValue("heldItem", item.id);
            }
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
