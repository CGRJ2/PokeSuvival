using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : Singleton<UIManager>
{
    [field: SerializeField] public UIGroup_Initialize InitializeGroup { get; private set; }
    [field: SerializeField] public UIGroup_Static StaticGroup { get; private set; }
    [field: SerializeField] public UIGroup_Lobby LobbyGroup { get; private set; }
    [field: SerializeField] public UIGroup_InGame InGameGroup { get; private set; }

    Stack<GameObject> activedPanelStack = new Stack<GameObject>();

    [SerializeField] List<GameObject> DebugStackView = new List<GameObject>();


    public void Init()
    {
        base.SingletonInit();

        InitializeGroup.Init();
        StaticGroup.Init();
        LobbyGroup.Init();
        InGameGroup.Init();

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

        // 디버그용
        DebugStackView = activedPanelStack.ToList();
    }

    public void ClosePanel()
    {
        activedPanelStack.Pop().SetActive(false);

        // 디버그용
        DebugStackView = activedPanelStack.ToList();
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

        // 디버그용
        DebugStackView = activedPanelStack.ToList();
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

        // 디버그용
        DebugStackView = activedPanelStack.ToList();
    }

    public void OnEsc()
    {
        if (activedPanelStack.Count > 0)
            ClosePanel();
    }

}
