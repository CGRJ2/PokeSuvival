using UnityEngine;

public class DirectionalTeleporter : MonoBehaviour
{
    public enum TeleportDirection { Left, Right, Top, Bottom }

    [SerializeField] private TeleportDirection direction;
    [SerializeField] private float teleportCoordinate = 74f; // �ڷ���Ʈ�� ��ǥ�� (�⺻�� 74)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player �±׸� ���� ������Ʈ�� ó��
        if (collision.CompareTag("Player"))
        {
            Vector3 playerPosition = collision.transform.position;

            // ���⿡ ���� ������ ��ǥ ����
            switch (direction)
            {
                case TeleportDirection.Left:
                    playerPosition.x = -teleportCoordinate; // ���� �ڷ�����: x = -74
                    break;

                case TeleportDirection.Right:
                    playerPosition.x = teleportCoordinate;  // ������ �ڷ�����: x = 74
                    break;

                case TeleportDirection.Top:
                    playerPosition.y = teleportCoordinate;  // ���� �ڷ�����: y = 74
                    break;

                case TeleportDirection.Bottom:
                    playerPosition.y = -teleportCoordinate; // �Ʒ��� �ڷ�����: y = -74
                    break;
            }

            // �÷��̾� ��ġ ����
            collision.transform.position = playerPosition;

            // ����� �α� (�ʿ�� �ּ� ����)
            // Debug.Log($"�÷��̾ {direction} ���� ���� �ڷ���Ʈ�߽��ϴ�: {playerPosition}");
        }
    }
}