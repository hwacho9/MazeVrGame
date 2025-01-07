using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public void StartSinglePlayer()
    {
        Debug.Log("싱글플레이 버튼이 클릭되었습니다!"); // 로그 출력
        // MazeScene으로 이동
        SceneManager.LoadScene("MazeScene");
    }
}
