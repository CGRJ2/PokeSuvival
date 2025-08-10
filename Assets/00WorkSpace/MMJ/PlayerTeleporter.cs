using UnityEngine;

public class DirectionalTeleporter : MonoBehaviour
{
    public enum TeleportDirection { Left, Right, Top, Bottom }

    [SerializeField] private TeleportDirection direction;
    [SerializeField] private float teleportCoordinate = 74f; // 텔레포트할 좌표값 (기본값 74)

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Player 태그를 가진 오브젝트만 처리
        if (collision.CompareTag("Player"))
        {
            Vector3 playerPosition = collision.transform.position;

            // 방향에 따라 적절한 좌표 설정
            switch (direction)
            {
                case TeleportDirection.Left:
                    playerPosition.x = -teleportCoordinate; // 왼쪽 텔레포터: x = -74
                    break;

                case TeleportDirection.Right:
                    playerPosition.x = teleportCoordinate;  // 오른쪽 텔레포터: x = 74
                    break;

                case TeleportDirection.Top:
                    playerPosition.y = teleportCoordinate;  // 위쪽 텔레포터: y = 74
                    break;

                case TeleportDirection.Bottom:
                    playerPosition.y = -teleportCoordinate; // 아래쪽 텔레포터: y = -74
                    break;
            }

            // 플레이어 위치 변경
            collision.transform.position = playerPosition;

            // 디버그 로그 (필요시 주석 해제)
            // Debug.Log($"플레이어를 {direction} 방향 경계로 텔레포트했습니다: {playerPosition}");
        }
    }
}