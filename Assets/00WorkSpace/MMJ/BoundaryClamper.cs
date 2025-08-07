using UnityEngine;

public class BoundaryClamper : MonoBehaviour
{
    // 맵의 경계 좌표를 설정합니다. Inspector에서 조정해주세요.
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    // 이 트리거에 들어올 게임 오브젝트의 태그를 설정합니다. (예: "Player")
    public string targetTag = "Player";

    private void OnTriggerStay2D(Collider2D other) // OnTriggerEnter2D도 가능하지만, 돌진 시 안전하게 OnTriggerStay2D 추천
    {
        // 대상 오브젝트가 설정된 태그와 일치하는지 확인합니다.
        if (other.CompareTag(targetTag))
        {
            Vector3 clampedPosition = other.transform.position;

            // X 좌표를 맵 경계 내로 제한합니다.
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            // Y 좌표를 맵 경계 내로 제한합니다.
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

            // 클램프된 위치를 대상 오브젝트에 적용합니다.
            other.transform.position = clampedPosition;
        }
    }
}