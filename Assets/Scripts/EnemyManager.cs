using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MazeGenerator;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;  // 적 프리팹
    public GameObject xrRig;        // 플레이어 (XR Rig)
    public Text gameOverText;       // 게임 오버 메시지

    private List<GameObject> enemies = new List<GameObject>();

    /// <summary>
    /// 적 생성
    /// </summary>
    public void SpawnEnemies(int count, TileType[,] tile, int mazeSize, Transform mazeParent)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomEmptyPosition(tile, mazeSize);
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, mazeParent);
            enemies.Add(enemy);
        }
    }

    /// <summary>
    /// 매 프레임마다 플레이어와 적 충돌 확인
    /// </summary>
    void Update()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            // 적과 플레이어의 거리 계산
            float distance = Vector3.Distance(enemy.transform.position, xrRig.transform.position);

            if (distance < 0.5f) // 충돌 시
            {
                GameOver();
                break;
            }
        }
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOverText.gameObject.SetActive(true);
        Time.timeScale = 0; // 게임 멈춤
    }

    /// <summary>
    /// 빈 타일에서 무작위 위치 반환
    /// </summary>
    private Vector3 GetRandomEmptyPosition(TileType[,] tile, int mazeSize)
    {
        int x, y;

        while (true)
        {
            x = Random.Range(1, mazeSize - 1);
            y = Random.Range(1, mazeSize - 1);

            if (tile[y, x] == TileType.Empty) // 빈 칸이면 위치 반환
            {
                return new Vector3(x, 0, y);
            }
        }
    }
}
