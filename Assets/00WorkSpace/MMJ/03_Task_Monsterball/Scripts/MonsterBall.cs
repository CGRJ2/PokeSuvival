using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class MonsterBall : MonoBehaviourPunCallbacks
{
    [Header("아이템 설정")]
    [SerializeField] private GameObject itemPrefab; // 생성할 아이템 프리팹

    // 플레이어 공격에 의해 파괴될 때 호출
    public void OnHit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 몬스터볼의 현재 위치 저장
            Vector3 spawnPosition = transform.position;

            // 아이템 드롭 로직
            photonView.RPC("RPC_DropItem", RpcTarget.All);

            // 풀로 반환
            MonsterBallPoolManager.Instance.ReturnToPool(gameObject);
        }
    }

    [PunRPC]
    private void RPC_DropItem(Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("아이템 프리팹이 할당되지 않았습니다!");
            return;
        }

        // 몬스터볼이 있던 위치에 아이템 생성
        GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);

        Debug.Log("아이템이 생성되었습니다!");
    }
}
