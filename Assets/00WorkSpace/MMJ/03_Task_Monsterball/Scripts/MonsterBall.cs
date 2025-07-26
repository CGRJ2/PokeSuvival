using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;


public class MonsterBall : MonoBehaviourPun, IPunObservable
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

            // 아이템 드롭 로직 - 위치 정보를 함께 전달
            photonView.RPC(nameof(RPC_DropItem), RpcTarget.AllBuffered, spawnPosition);

            // 풀로 반환
            MonsterBallPoolManager.Instance.ReturnToPool(gameObject);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내 데이터를 보냄
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // 상대방의 데이터를 받음
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
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
        // [[버그 해결을 위한 주석처리333]]GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);
        GameObject newItem = PhotonNetwork.Instantiate("ItemPrefab", position, Quaternion.identity);

        Debug.Log("아이템이 생성되었습니다!");
    }
}
