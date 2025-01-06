using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;                // �̷��� ũ�� (Ȧ�� ����)
    public GameObject wallPrefab;        // �� ������
    public GameObject floorPrefab;       // �ٴ� ������
    public GameObject exitPrefab;        // ���� ���� ������
    public GameObject xrRig;             // XR Rig (�÷��̾�)
    public RawImage miniMapImage;        // �̴ϸ��� ǥ���� UI �̹���
    public EnemyManager enemyManager;    // EnemyManager ����

    private TileType[,] tile;            // �̷� �迭 (Wall, Empty)
    private Vector3 startPosition;       // ���� ����
    private Vector3 exitPosition;        // ���� ����
    private Texture2D miniMapTexture;    // �̴ϸ� �ؽ�ó
    private Vector2Int lastPlayerPosition; // ���� �÷��̾� ��ġ

    public enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size);    // �̷� ����
        DrawMaze();          // �̷� �׸���
        EnsurePath();        // ��ΰ� ����ǵ��� ó��
        PlaceExit();         // ���� ���� ����
        PlaceXRRig();        // XR Rig�� ���������� �̵�
        GenerateMiniMap();   // �̴ϸ� ����

        // �� ���� ��û
        enemyManager.SpawnEnemies(3, tile, size, transform);
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // �� �����Ӹ��� �̴ϸʿ��� �÷��̾� ��ġ ����
    }

    /// <summary>
    /// �̷� �ʱ�ȭ
    /// </summary>
    public void Initialize(int size)
    {
        if (size % 2 == 0)
        {
            Debug.LogError("�̷� ũ��� Ȧ������ �մϴ�.");
            return;
        }

        tile = new TileType[size, size];

        // Binary Tree �˰������� �̷� ����
        GenerateByBinaryTree();

        // ���� ������ ���� ���� ����
        startPosition = new Vector3(1, 0, 1);                 // (1,1)
        exitPosition = new Vector3(size - 2, 0, size - 2);   // (size-2, size-2)
    }

    /// <summary>
    /// Binary Tree �˰������� �̷� ����
    /// </summary>
    void GenerateByBinaryTree()
    {
        // 1) ��� ���� �ϴ� ������ �ʱ�ȭ
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tile[y, x] = TileType.Wall;
            }
        }

        // 2) ����(Ȧ�� ��ǥ)�� ���ؼ� Empty�� �����
        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                tile[y, x] = TileType.Empty;
            }
        }

        // 3) Binary Tree ��Ģ ����: �� (Ȧ��, Ȧ��) ������ ���� or ���� �� �㹰��
        System.Random rand = new System.Random();

        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                if (y == 1 && x == 1)
                {
                    continue; // (1,1)�� �ƹ� �͵� ���� ����
                }
                else if (y == 1) // �� �� ���̸� ���ʸ� �ձ�
                {
                    CarveWest(x, y);
                }
                else if (x == 1) // �� ���� ���̸� ���ʸ� �ձ�
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

    /// <summary>
    /// ���� �� �㹰��
    /// </summary>
    void CarveNorth(int x, int y)
    {
        if (y - 1 >= 0 && tile[y - 1, x] == TileType.Wall)
        {
            tile[y - 1, x] = TileType.Empty;
        }
    }

    /// <summary>
    /// ���� �� �㹰��
    /// </summary>
    void CarveWest(int x, int y)
    {
        if (x - 1 >= 0 && tile[y, x - 1] == TileType.Wall)
        {
            tile[y, x - 1] = TileType.Empty;
        }
    }

    /// <summary>
    /// ��� ���� (BFS�� ������~������ ���� Ȯ��)
    /// </summary>
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

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// �÷��̾�(XR Rig) ������ġ�� �̵�
    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition;
        }
    }

    /// <summary>
    void GenerateMiniMap()
    {
        int pixelScale = 1; // 1 Ÿ�ϴ� �ȼ� ũ�� (Ȯ�� ����)
        int mapSize = size * pixelScale; // �ؽ�ó ũ�� = �̷� ũ�� * �ȼ� ũ��

        // �ؽ�ó �ʱ�ȭ
        miniMapTexture = new Texture2D(mapSize, mapSize);
        miniMapTexture.filterMode = FilterMode.Point;
        miniMapTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // ���� ����: ��, ���� ����, ���� ����, ���
                Color color = tile[y, x] == TileType.Wall ? Color.black : Color.white;
                if (x == 1 && y == 1) color = Color.green; // ���� ����
                if (x == size - 2 && y == size - 2) color = Color.red; // ���� ����

                // �ȼ� Ȯ��: �� Ÿ���� ���� �ȼ��� ä��
                for (int dy = 0; dy < pixelScale; dy++)
                {
                    for (int dx = 0; dx < pixelScale; dx++)
                    {
                        miniMapTexture.SetPixel(x * pixelScale + dx, y * pixelScale + dy, color);
                    }
                }
            }
        }

        miniMapTexture.Apply(); // �ؽ�ó ����
        miniMapImage.texture = miniMapTexture; // UI �̹����� �ؽ�ó ����
    }


    /// <summary>
    /// �̴ϸʿ��� �÷��̾��� ��ġ�� �� ������ ����
    /// </summary>
    private Color lastPlayerTileColor; // ���� �÷��̾� ��ġ�� ���� ���� ����

    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // ���� �÷��̾� ��ġ�� �̷��� ��ǥ�� ��ȯ
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // ���� ��ġ�� ���� �������� �ǵ���
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerTileColor);
        }

        // ���� ��ġ�� ���� ������ ����
        lastPlayerTileColor = miniMapTexture.GetPixel(playerX, playerY);

        // ���� ��ġ�� �Ķ������� ǥ��
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);

        miniMapTexture.Apply(); // �ؽ�ó ����

        // ���� ��ġ ������Ʈ
        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }


}