using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Game Over!"); // ���� ���� ó��
            // �߰�: UI ǥ�� �Ǵ� �� ���� ����
        }
    }
}
