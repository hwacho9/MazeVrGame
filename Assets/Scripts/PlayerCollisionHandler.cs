using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Game Over!"); // 게임 오버 처리
            // 추가: UI 표시 또는 씬 리셋 로직
        }
    }
}
