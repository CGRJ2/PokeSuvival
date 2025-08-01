using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrbPool : MonoBehaviourPun
{
    public static ExpOrbPool Instance { get; private set; }

    [SerializeField] private ExpOrb orbPrefab;

    [SerializeField] private int initialPoolSize = 50;

    private Queue<ExpOrb> pool = new Queue<ExpOrb>();

    // SJH
    private Queue<ExpOrb> _networkPool = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //InitPool(initialPoolSize);
	}

    public void InitPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ExpOrb orb = Instantiate(orbPrefab);
            orb.gameObject.SetActive(false);
            pool.Enqueue(orb);
        }
    }

    //public ExpOrb GetOrb()
    //{
    //    if (pool.Count > 0)
    //    {
    //        return pool.Dequeue();
    //    }

    //    // 일단~ 부족하면 새로 생성
    //    ExpOrb orb = Instantiate(orbPrefab, parentTransform);
    //    orb.gameObject.SetActive(false);
    //    return orb;
    //}

    // SJH
	public ExpOrb GetOrb()
	{
		if (!PhotonNetwork.IsMasterClient) return null;
		if (_networkPool.Count > 0)
		{
			return _networkPool.Dequeue();
		}

        // 일단~ 부족하면 새로 생성
        GameObject go = PhotonNetwork.InstantiateRoomObject("ExpOrbParents", new Vector3(999999, 999999), Quaternion.identity);
		ExpOrb orb = go.GetComponent<ExpOrb>();
		orb.gameObject.SetActive(false);
		return orb;
	}

	public void ReturnOrb(ExpOrb orb)
    {
		if (!PhotonNetwork.IsMasterClient) return;
		orb.gameObject.SetActive(false);
        //pool.Enqueue(orb);
        _networkPool.Enqueue(orb);
    }

    public void NetworkInit()
    {
		if (!PhotonNetwork.IsMasterClient) return;
		for (int i = 0; i < initialPoolSize; i++)
		{
			GameObject go = PhotonNetwork.InstantiateRoomObject("ExpOrb", new Vector3(999999, 999999), Quaternion.identity);
            if (go == null)
            {
                Debug.LogError("게임 오브젝트 null");
            }
			ExpOrb orb = go.GetComponent<ExpOrb>();
            if (orb == null)
            {
                Debug.LogError("ExpOrb null");
            }
			go.gameObject.SetActive(false);
			_networkPool.Enqueue(orb);
		}
	}
}
