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
    public GameObject xrRig;            // XR Rig (�÷��̾�)
    public RawImage miniMapImage;        // �̴ϸ��� ǥ���� UI �̹���

    private TileType[,] tile;            // �̷� �迭 (Wall, Empty)
    private Vector3 startPosition;       // ���� ����
    private Vector3 exitPosition;        // ���� ����
    private Texture2D miniMapTexture;    // �̴ϸ� �ؽ�ó
    private Vector2Int lastPlayerPosition; // ���� �÷��̾� ��ġ

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size);    // �̷� ����
        DrawMaze();          // �̷� �׸���
        EnsurePath();        // ��ΰ� ����ǵ��� ó��
        PlaceExit();         // ���� ���� ����
        PlaceXRRig();        // XR Rig�� ���������� �̵�
        GenerateMiniMap();   // �̴ϸ� ����
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // �̴ϸʿ��� �÷��̾� ��ġ ����
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
                // �� ù ���̸� �����ʡ��� ���� �� ����
                // �� ù ���̸� �����ʡ��� ���� �� ����
                // ���� Binary Tree������ "���� Ȥ�� ����"�� �����ϴ� ������ ������
                // ���⼭�� "���� Ȥ�� ����"���� ���ø� ���Կ�.

                // ������ 0 �Ǵ� 1�� ���� ���� (0: ��, 1: ��)
                int direction = rand.Next(0, 2);

                // ���� y == 1 �̶�� �������� ���� �� ���� -> ���� �ձ�
                // ���� x == 1 �̶�� �������� ���� �� ���� -> ���� �ձ�
                // �� �� �����ϸ� ��������

                if (y == 1 && x == 1)
                {
                    // (1,1)�� �ƹ��͵� ���� �ʰų�
                    // Ȥ�� �Ʒ���/�������� �̹� Empty�� �����Ǿ����� �״�� �ξ ��.
                }
                else if (y == 1) // �� �� ���̸� ������ �̹� ���� ���̹Ƿ� ���ʸ� ����
                {
                    CarveWest(x, y);
                }
                else if (x == 1) // �� ���� ���̸� ������ �̹� ���� ���̹Ƿ� ���ʸ� ����
                {
                    CarveNorth(x, y);
                }
                else
                {
                    if (direction == 0)
                    {
                        // ���� �ձ�
                        CarveNorth(x, y);
                    }
                    else
                    {
                        // ���� �ձ�
                        CarveWest(x, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// �ش� ��(x, y)�� ������ ������ �㹫�� �Լ�
    /// </summary>
    void CarveNorth(int x, int y)
    {
        // y-1 ��ġ�� ���̶�� �մ´�
        if (y - 1 >= 0 && tile[y - 1, x] == TileType.Wall)
        {
            tile[y - 1, x] = TileType.Empty;
        }
    }

    /// <summary>
    /// �ش� ��(x, y)�� ������ ������ �㹫�� �Լ�
    /// </summary>
    void CarveWest(int x, int y)
    {
        // x-1 ��ġ�� ���̶�� �մ´�
        if (x - 1 >= 0 && tile[y, x - 1] == TileType.Wall)
        {
            tile[y, x - 1] = TileType.Empty;
        }
    }

    /// <summary>
    /// �̷θ� ���� ���� ������Ʈ�� �׷��ִ� �Լ�
    /// </summary>
    void DrawMaze()
    {
        // Ȥ�� ������ ������� �ڽ� ������Ʈ�� ������ ����
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
    /// ��ΰ� ������ ������ �վ��ִ� �Լ�
    /// </summary>
    void EnsurePath()
    {
        // BFS�� (1,1)���� (size-2, size-2)�� �� �� �ִ��� Ȯ��
        if (!CheckIfPathExists())
        {
            // ��ΰ� ���ٸ� ������ �������� �մ� ����
            CarveDirectPath();
            // ���� �վ����� �ٽ� �׷���
            DrawMaze();
        }
    }

    /// <summary>
    /// BFS�� ������~������ ��� ���� ���� üũ
    /// </summary>
    bool CheckIfPathExists()
    {
        Vector2Int startCell = new Vector2Int(1, 1);
        Vector2Int exitCell = new Vector2Int(size - 2, size - 2);

        bool[,] visited = new bool[size, size];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCell);
        visited[startCell.y, startCell.x] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == exitCell)
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

    /// <summary>
    /// ������~������ ���̿� �������� ���� �մ� ����
    /// </summary>
    void CarveDirectPath()
    {
        // (1,1)���� ���������� �� ���� ��, �Ʒ��� �� �մ´�
        for (int x = 1; x < size - 1; x++)
        {
            tile[1, x] = TileType.Empty;
        }
        for (int y = 1; y < size - 1; y++)
        {
            tile[y, size - 2] = TileType.Empty;
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
    /// </summary>
    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition;
        }
    }

    /// <summary>
    /// �̴ϸ� �ؽ�ó ���� �� UI ����
    /// </summary>
    void GenerateMiniMap()
    {
        miniMapTexture = new Texture2D(size, size);
        miniMapTexture.filterMode = FilterMode.Point;
        miniMapTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (tile[y, x] == TileType.Wall)
                    miniMapTexture.SetPixel(x, y, Color.black);
                else if (x == 1 && y == 1)
                    miniMapTexture.SetPixel(x, y, Color.green); // ���� ����
                else if (x == size - 2 && y == size - 2)
                    miniMapTexture.SetPixel(x, y, Color.red);   // ���� ����
                else
                    miniMapTexture.SetPixel(x, y, Color.white);
            }
        }

        miniMapTexture.Apply();
        miniMapImage.texture = miniMapTexture;
    }

    /// <summary>
    /// �̴ϸʿ��� �÷��̾��� ��ġ�� �� ������ ����
    /// </summary>
    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // ���� �÷��̾� ��ġ
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // ���� ��ġ�� ������� �ǵ���
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, Color.white);
        }

        // ���� ��ġ�� �Ķ������� ǥ��
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);
        miniMapTexture.Apply();

        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }
}
