using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static MazeGenerator;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;  // �� ������
    public GameObject xrRig;        // �÷��̾� (XR Rig)
    public Text gameOverText;       // ���� ���� �޽���

    private List<GameObject> enemies = new List<GameObject>();

    /// <summary>
    /// �� ����
    /// </summary>
    public void SpawnEnemies(int count, TileType[,] tile, int mazeSize, Transform mazeParent)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomEmptyPosition(tile, mazeSize);

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, mazeParent);

            // NavMeshAgent ������Ʈ Ȯ�� �� �ʱ�ȭ
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.speed = 3.0f; // �� �̵� �ӵ� ����
            }
            else
            {
                Debug.LogError("NavMeshAgent�� ���� �߰����� �ʾҰų� NavMesh ���� ��ġ���� �ʾҽ��ϴ�!");
            }

            enemies.Add(enemy);
        }
    }

    /// <summary>
    /// �� �����Ӹ��� �÷��̾�� �� �浹 Ȯ��
    /// </summary>
    void Update()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // �÷��̾� ��ġ�� ���� ��ǥ�� ����
                agent.SetDestination(xrRig.transform.position);
            }

            // ���� �÷��̾��� �Ÿ� ���
            float distance = Vector3.Distance(enemy.transform.position, xrRig.transform.position);

            if (distance < 0.5f) // �浹 ��
            {
                GameOver();
                break;
            }
        }
    }

    /// <summary>
    /// ���� ���� ó��
    /// </summary>
    void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOverText.gameObject.SetActive(true);
        Time.timeScale = 0; // ���� ����
    }

    /// <summary>
    /// �� Ÿ�Ͽ��� ������ ��ġ ��ȯ
    /// </summary>
    private Vector3 GetRandomEmptyPosition(TileType[,] tile, int mazeSize)
    {
        int x, y;

        while (true)
        {
            x = Random.Range(1, mazeSize - 1);
            y = Random.Range(1, mazeSize - 1);

            if (tile[y, x] == TileType.Empty) // �� ĭ�̸� ��ġ ��ȯ
            {
                return new Vector3(x, 0, y); // NavMesh Bake ���̿� �°� Y�� ����
            }
        }
    }
}
