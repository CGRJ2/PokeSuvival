using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    [SerializeField] private List<GameObject> _statusList = new List<GameObject>();

    public void OnStatus(StatusType status)
    {
        GameObject go = _statusList[(int)status];
        if (go == null) return;

        Debug.Log($"{status} 활성화");
        go.SetActive(true);
    }

    public void OffStatus(StatusType status)
    {
		GameObject go = _statusList[(int)status];
		if (go == null) return;

		Debug.Log($"{status} 비활성화");
		go.SetActive(false);
	}

    public void StatusEffectClear()
    {
        Debug.Log("상태이상 이펙트 전부 비활성화");
        foreach (GameObject go in _statusList)
        {
            if (go == null) continue;
            go.SetActive(false);
        }
    }
}
