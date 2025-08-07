using UnityEngine;
using UnityEngine.UI;

public class PlayerInstanceUI : MonoBehaviour
{
    PlayerController pc;
    [SerializeField] PlayerHeadUI headUI;
    [SerializeField] Image image_MiniMapPoint;
    [SerializeField] Canvas canvasSelf;

    private void Awake() => Init();

    public void Init()
    {
        pc = GetComponentInParent<PlayerController>();
        pc.OnBuffUpdate += UIManager.Instance.InGameGroup.panel_HUD.panel_BuffState.ActiveBuffUIView;
    }

    private void Update()
    {
        if (pc.Model != null)
        {
            headUI.UpdateView(pc.Model);

            // 본인이라면
            if (pc.Model.UserId == NetworkManager.Instance.GetUserId())
            {
                image_MiniMapPoint.color = Color.green;
                canvasSelf.sortingOrder = 90;
            }
            // 적들이라면
            else
            {
                image_MiniMapPoint.color = Color.red;
                canvasSelf.sortingOrder = 80;
            }
        }
    }

    private void OnEnable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.activedPlayerList.Add(pc);
    }
    private void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InGameGroup.activedPlayerList.Remove(pc);
    }

}
