using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [field: SerializeField] public UIGroup_Initialize InitializeGroup { get; private set; }
    [field: SerializeField] public UIGroup_Loading LoadingGroup { get; private set; }
    [field: SerializeField] public UIGroup_Lobby LobbyGroup { get; private set; }
    [field: SerializeField] public UIGroup_InGame InGameGroup { get; private set; }

    Stack<GameObject> activedPanelStack = new Stack<GameObject>();

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
        InitializeGroup.Init();
        LoadingGroup.Init();
        LobbyGroup.Init();
    }


    public void OpenPanel(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void ClosePanel(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
