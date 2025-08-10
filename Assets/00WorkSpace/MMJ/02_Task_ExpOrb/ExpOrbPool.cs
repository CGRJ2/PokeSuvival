using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrbPool : MonoBehaviourPun
{
    public static ExpOrbPool Instance { get; private set; }

    [SerializeField] private ExpOrb orbPrefab;

    [SerializeField] private int initialPoolSize = 50;
    // SJH
    public Queue<ExpOrb> _networkPool = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
	}

    // SJH
	public ExpOrb GetOrb()
	{
		if (!PhotonNetwork.IsMasterClient) return null;
		if (_networkPool.Count > 0)
		{
			return _networkPool.Dequeue();
		}

        // 일단~ 부족하면 새로 생성
        GameObject go = PhotonNetwork.InstantiateRoomObject("ExpOrb", new Vector3(999999, 999999), Quaternion.identity);
		ExpOrb orb = go.GetComponent<ExpOrb>();
		orb.gameObject.SetActive(false);
		return orb;
	}

	public void ReturnOrb(ExpOrb orb)
    {
		if (!PhotonNetwork.IsMasterClient) return;
		orb.gameObject.SetActive(false);
        _networkPool.Enqueue(orb);
    }

    public void NetworkInit()
    {
		if (!PhotonNetwork.IsMasterClient) return;
		for (int i = 0; i < initialPoolSize; i++)
		{
			GameObject go = PhotonNetwork.InstantiateRoomObject("ExpOrb", new Vector3(999999, 999999), Quaternion.identity, 0, new object[] { });
            if (go == null)
            {
                Debug.LogError("게임 오브젝트 null");
            }
			ExpOrb orb = go.GetComponent<ExpOrb>();
            if (orb == null)
            {
                Debug.LogError("ExpOrb null");
            }
            
            if (ExpOrbSpawner.Instance != null)
                orb.spawner = ExpOrbSpawner.Instance;

			go.gameObject.SetActive(false);
			_networkPool.Enqueue(orb);
		}
	}
}
