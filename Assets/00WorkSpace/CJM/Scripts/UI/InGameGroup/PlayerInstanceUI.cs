using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInstanceUI : MonoBehaviour
{
    PlayerController pc;
    [SerializeField] PlayerHeadUI headUI;

    private void Awake() => Init();

    public void Init()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (pc.Model != null)
            headUI.UpdateView(pc.Model);
    }

    private void OnEnable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.panel_HUD.panel_InGameServerRanking.activedPlayerList.Add(pc);
    }
    private void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.panel_HUD.panel_InGameServerRanking.activedPlayerList.Remove(pc);
    }

}
