using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [field: SerializeField] public UIGroup_Initialize InitializeGroup { get; private set; }
    [field: SerializeField] public UIGroup_Static StaticGroup { get; private set; }
    [field: SerializeField] public UIGroup_Lobby LobbyGroup { get; private set; }
    [field: SerializeField] public UIGroup_InGame InGameGroup { get; private set; }

    Stack<GameObject> activedPanelStack = new Stack<GameObject>();

    public void Init()
    {
        base.SingletonInit();

        InitializeGroup.Init();
        StaticGroup.Init();
        LobbyGroup.Init();
        InGameGroup.Init();
    }

    private void Start()
    {
        EventBind();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activedPanelStack.Count > 0)
            {
                ClosePanel();
            }
            else
            {
                UIManager.Instance.StaticGroup.panel_UpperMenu.SwitchToggleDropDownButton();
            }
        }
    }

    private void OnDestroy()
    {
        //pauseAction.performed -= OnEsc;
        //pauseAction.Disable();
    }

    public void OpenPanel(GameObject gameObject)
    {
        gameObject.SetActive(true);
        activedPanelStack.Push(gameObject);
    }

    public void ClosePanel()
    {
        activedPanelStack.Pop().SetActive(false);
    }

    public void ClosePanel(GameObject gameObject)
    {
        if (activedPanelStack.Peek() == gameObject)
        {
            ClosePanel();
            return;
        }
        else
        {
            gameObject.SetActive(false);
            List<GameObject> tempList = activedPanelStack.ToList();
            tempList.Remove(gameObject);
            tempList.Reverse();
            activedPanelStack = new Stack<GameObject>(tempList);
        }
    }

    public void CloseAllActivedPanels()
    {
        foreach (GameObject panel in activedPanelStack)
        {
            panel.SetActive(false);
        }

        ClearPanelStack();
    }

    public void ClearPanelStack()
    {
        activedPanelStack.Clear();
    }

    public void OnEsc()
    {
        if (activedPanelStack.Count > 0)
            ClosePanel();
    }


    public void EventBind()
    {
        NetworkManager netManager = NetworkManager.Instance;

        netManager.LoadingEvent.AddListener( (flag) => StaticGroup.panel_Loading.gameObject.SetActive(flag));

        netManager.PlayerFirstEnterEvent.AddListener(InitializeGroup.InitView);

        netManager.InGameEnterEvent.AddListener(() =>
        {
            CloseAllActivedPanels();
            StaticGroup.SetDefaultSettings();
            LobbyGroup.gameObject.SetActive(false);
            InGameGroup.gameObject.SetActive(true);
            InGameGroup.GameStartViewUpdate();
        });

        netManager.LobbyEnterEvent.AddListener(() =>
        {
            if (StaticGroup.panel_CustomBGM.IsBGMNullOrInitial())
                StaticGroup.panel_CustomBGM.SetNewAudioClipAndPlay(LobbyGroup.LobbyDefaultBGM);

            LobbyGroup.gameObject.SetActive(true);
            LobbyGroup.panel_RoomInside.gameObject.SetActive(false);

            LobbyGroup.OnJoinedLobbyDefaultSetting();
            CloseAllActivedPanels();
            LobbyGroup.panel_LobbyDefault.panel_PokemonView.UpdateView();

            InitializeGroup.gameObject.SetActive(false);
            InGameGroup.gameObject.SetActive(false);
            
            // 플레이어 정보 업데이트
            if (BackendManager.Auth.CurrentUser != null)
            {
                //Debug.Log("로비씬 플레이어 정보 갱신");
                LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdatePlayerInfoView();
                LobbyGroup.panel_LobbyDefault.panel_PlayerRecords.UpdateView();
            }
            else
            {
                //Debug.Log("로비씬 플레이어 정보 없으니 게스트 버전 업데이트");
                LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.ClearView();
                LobbyGroup.panel_LobbyDefault.panel_PlayerInfo.UpdateGuestInfoView();
                LobbyGroup.panel_LobbyDefault.panel_PlayerRecords.UpdateView();
            }
        });


        netManager.RoomEnterEvent.AddListener(() =>
        {
            LobbyGroup.panel_RoomInside.gameObject.SetActive(true);
            CloseAllActivedPanels();
            LobbyGroup.panel_RoomInside.InitRoomView();
            LobbyGroup.panel_RoomInside.UpdatePlayerList();
        });

        netManager.RoomUpdateEvent.AddListener(() =>
        {
            LobbyGroup.panel_RoomInside.UpdatePlayerList();
            LobbyGroup.panel_RoomInside.panel_MapSettings.UpdateRoomProperty();
        });
    }
}
