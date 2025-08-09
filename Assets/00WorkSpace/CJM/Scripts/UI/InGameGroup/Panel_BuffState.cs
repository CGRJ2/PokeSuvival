using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_BuffState : MonoBehaviour
{
    Slot_Buff[] slots;
    [SerializeField] Transform slotsParent;
    public void Init()
    {
        slots = slotsParent.GetComponentsInChildren<Slot_Buff>();

        InitSlots();
    }

    public void InitSlots()
    {
        // 초기화 시, 모든 버프 슬롯 비활성화
        foreach (Slot_Buff slot in slots)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.InitSlotData();
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void ActiveBuffUIView(Sprite sprite, float duration)
    {
        foreach(Slot_Buff slot_Buff in slots)
        {
            if (!slot_Buff.gameObject.activeSelf)
            {
                slot_Buff.UpdateBuffSlot(sprite, duration);
                break;
            }
        }
    }
}
