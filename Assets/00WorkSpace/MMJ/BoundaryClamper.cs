using UnityEngine;

public class BoundaryClamper : MonoBehaviour
{
    // ���� ��� ��ǥ�� �����մϴ�. Inspector���� �������ּ���.
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    // �� Ʈ���ſ� ���� ���� ������Ʈ�� �±׸� �����մϴ�. (��: "Player")
    public string targetTag = "Player";

    private void OnTriggerStay2D(Collider2D other) // OnTriggerEnter2D�� ����������, ���� �� �����ϰ� OnTriggerStay2D ��õ
    {
        // ��� ������Ʈ�� ������ �±׿� ��ġ�ϴ��� Ȯ���մϴ�.
        if (other.CompareTag(targetTag))
        {
            Vector3 clampedPosition = other.transform.position;

            // X ��ǥ�� �� ��� ���� �����մϴ�.
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            // Y ��ǥ�� �� ��� ���� �����մϴ�.
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

            // Ŭ������ ��ġ�� ��� ������Ʈ�� �����մϴ�.
            other.transform.position = clampedPosition;
        }
    }
}