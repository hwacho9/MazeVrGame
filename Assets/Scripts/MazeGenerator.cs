using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;                // �̷��� ũ�� (Ȧ�� ����)
    public GameObject wallPrefab;        // �� ������
    public GameObject floorPrefab;       // �ٴ� ������
    public GameObject exitPrefab;        // ���� ���� ������
    public GameObject xrRig;             // XR Rig (�÷��̾�)
    public RawImage miniMapImage;        // �̴ϸ��� ǥ���� UI �̹���
    public EnemyManager enemyManager;    // EnemyManager ����
    public TextMeshProUGUI endGameText;             // ���� �޽��� UI
    public Button restartButton;         // ����� ��ư UI

    private TileType[,] tile;            // �̷� �迭 (Wall, Empty)
    private Vector3 startPosition;       // ���� ����
    private Vector3 exitPosition;        // ���� ����
    private Texture2D miniMapTexture;    // �̴ϸ� �ؽ�ó
    private Vector2Int lastPlayerPosition; // ���� �÷��̾� ��ġ

    private Color lastPlayerTileColor;   // ���� �÷��̾� ��ġ�� ���� ���� ����

    public enum TileType { Wall, Empty };

    void Start()
    {
        // Time.timeScale 초기화
        Time.timeScale = 1;

        Initialize(size);    // �̷� ����
        DrawMaze();          // �̷� �׸���
        EnsurePath();        // ��ΰ� ����ǵ��� ó��
        PlaceExit();         // ���� ���� ����
        PlaceXRRig();        // XR Rig�� ���������� �̵�
        GenerateMiniMap();   // �̴ϸ� ����

        // �� ���� ��û
        enemyManager.SpawnEnemies(1, tile, size, transform);

        // UI �ʱ�ȭ
        endGameText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // �� �����Ӹ��� �̴ϸʿ��� �÷��̾� ��ġ ����
        CheckExitReached();      // ���� ���� ���� ���� Ȯ��
    }

    public void Initialize(int size)
    {
        if (size % 2 == 0)
        {
            Debug.LogError("�̷� ũ��� Ȧ������ �մϴ�.");
            return;
        }

        tile = new TileType[size, size];

        // Binary Tree �˰��������� �̷� ����
        GenerateByBinaryTree();

        // ���� ������ ���� ���� ����
        startPosition = new Vector3(1, 0, 1);                 // (1,1)
        exitPosition = new Vector3(size - 2, 0, size - 2);   // (size-2, size-2)
    }

    void GenerateByBinaryTree()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tile[y, x] = TileType.Wall;
            }
        }

        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                tile[y, x] = TileType.Empty;
            }
        }

        System.Random rand = new System.Random();

        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                if (y == 1 && x == 1)
                {
                    continue;
                }
                else if (y == 1)
                {
                    CarveWest(x, y);
                }
                else if (x == 1)
                {
                    CarveNorth(x, y);
                }
                else
                {
                    int direction = rand.Next(0, 2);
                    if (direction == 0)
                    {
                        CarveNorth(x, y);
                    }
                    else
                    {
                        CarveWest(x, y);
                    }
                }
            }
        }
    }

    void CarveNorth(int x, int y)
    {
        if (y - 1 >= 0 && tile[y - 1, x] == TileType.Wall)
        {
            tile[y - 1, x] = TileType.Empty;
        }
    }

    void CarveWest(int x, int y)
    {
        if (x - 1 >= 0 && tile[y, x - 1] == TileType.Wall)
        {
            tile[y, x - 1] = TileType.Empty;
        }
    }

    void EnsurePath()
    {
        if (!CheckIfPathExists())
        {
            CarveDirectPath();
            DrawMaze();
        }
    }

    bool CheckIfPathExists()
    {
        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = new Vector2Int(size - 2, size - 2);

        bool[,] visited = new bool[size, size];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.y, start.x] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
                return true;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx < 0 || nx >= size || ny < 0 || ny >= size)
                    continue;

                if (!visited[ny, nx] && tile[ny, nx] == TileType.Empty)
                {
                    visited[ny, nx] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return false;
    }

    void CarveDirectPath()
    {
        for (int x = 1; x < size - 1; x++)
        {
            tile[1, x] = TileType.Empty;
        }
        for (int y = 1; y < size - 1; y++)
        {
            tile[y, size - 2] = TileType.Empty;
        }
    }

    void DrawMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector3 position = new Vector3(x, 0, y);
                if (tile[y, x] == TileType.Wall)
                {
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform);
        }
    }

    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition;
        }
    }

    void GenerateMiniMap()
    {
        miniMapTexture = new Texture2D(size, size);
        miniMapTexture.filterMode = FilterMode.Point;
        miniMapTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color color = tile[y, x] == TileType.Wall ? Color.black : Color.white;
                if (x == 1 && y == 1) color = Color.green;
                if (x == size - 2 && y == size - 2) color = Color.red;
                miniMapTexture.SetPixel(x, y, color);
            }
        }

        miniMapTexture.Apply();
        miniMapImage.texture = miniMapTexture;
    }

    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerTileColor);
        }

        lastPlayerTileColor = miniMapTexture.GetPixel(playerX, playerY);
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);

        miniMapTexture.Apply();
        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }

    void CheckExitReached()
    {
        Vector3 playerPosition = xrRig.transform.position;
        float distance = Vector3.Distance(playerPosition, exitPosition);

        Debug.Log($"�÷��̾� ��ġ: {playerPosition}, ���� ����: {exitPosition}, �Ÿ�: {distance}");

        if (distance < 0.5f)
        {
            Debug.Log("�÷��̾ ���� ������ �����߽��ϴ�!"); // �α� ���
            EndGame();
        }
        else
        {
            Debug.Log("�÷��̾ ���� ���� ������ �������� �ʾҽ��ϴ�.");
        }
    }


    void EndGame()
    {
        // ���� �޽��� �� ����� ��ư Ȱ��ȭ
        if (endGameText != null && restartButton != null)
        {
            endGameText.text = "Ż�� ����! ���� ����!";
            endGameText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);

            // ���� ����
            Time.timeScale = 0;

            Debug.Log("���� ���� ȭ���� ǥ�õǾ����ϴ�.");
        }
        else
        {
            Debug.LogError("endGameText �Ǵ� restartButton�� �������� �ʾҽ��ϴ�. UI�� Ȯ���ϼ���.");
        }
    }


    public void RestartGame()
    {
        // ���� �� ����
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // �̴ϸ� �ؽ�ó �ʱ�ȭ
        if (miniMapTexture != null)
        {
            Destroy(miniMapTexture);
        }

        // Time.timeScale�� �ٽ� 1�� ����
        Time.timeScale = 1;

        // ���� �޽����� ��ư ����
        if (endGameText != null)
        {
            endGameText.gameObject.SetActive(false);
        }
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        // �̷� �����
        Initialize(size);    // �̷� �迭 �ʱ�ȭ
        DrawMaze();          // �̷� �׸���
        EnsurePath();        // ��� Ȯ��
        PlaceExit();         // ���� ���� ����
        PlaceXRRig();        // �÷��̾� ���� ��ġ�� �̵�
        GenerateMiniMap();   // �̴ϸ� �����

        // �� �����
        enemyManager.ClearEnemies(); // �� ��� �ʱ�ȭ
        enemyManager.SpawnEnemies(1, tile, size, transform);

        Debug.Log("�ʰ� ������ ����۵Ǿ����ϴ�!");
    }

}
