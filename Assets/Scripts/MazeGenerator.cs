using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;  // �̷��� ũ�� (Ȧ��)
    public GameObject wallPrefab;  // �� ������
    public GameObject floorPrefab; // �ٴ� ������
    public GameObject exitPrefab;  // ���� ���� ������
    public GameObject xrRig;       // XR Rig (�÷��̾�)

    private TileType[,] tile; // �̷� �迭
    private Vector3 startPosition; // ���� ����
    private Vector3 exitPosition;  // ���� ����

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size); // �̷� ����
        DrawMaze();       // �̷� �׸���
        EnsurePath();     // ���������� ���������� ��� ����
        PlaceExit();      // ���� ���� ����
        PlaceXRRig();     // XR Rig�� ���������� �̵�
    }

    public void Initialize(int size)
    {
        if (size % 2 == 0)
        {
            Debug.LogError("�̷� ũ��� Ȧ������ �մϴ�.");
            return;
        }

        tile = new TileType[size, size];

        // SideWinder �˰��� ���
        GenerateBySideWinder();

        // ���� ������ ���� ���� ����
        startPosition = new Vector3(1, 0, 1); // ���� ���� (x, y, z)
        exitPosition = new Vector3(size - 2, 0, size - 2); // ���� ���� (x, y, z)
    }

    void GenerateBySideWinder()
    {
        // ��� Ÿ���� ������ �ʱ�ȭ
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x % 2 == 0 || y % 2 == 0)
                    tile[y, x] = TileType.Wall;
                else
                    tile[y, x] = TileType.Empty;
            }
        }

        // SideWinder �˰������� ���� ����
        System.Random rand = new System.Random();
        for (int y = 1; y < size; y += 2)
        {
            int runStart = 1; // ���������� ����Ǵ� ���� ��ġ
            for (int x = 1; x < size; x += 2)
            {
                // �� ������ ��
                if (x == size - 2)
                {
                    if (y < size - 2) // �Ʒ��� ����
                        tile[y + 1, x] = TileType.Empty;
                }
                // �� ������ ��
                else if (y == size - 2)
                {
                    tile[y, x + 1] = TileType.Empty; // ���������� ����
                }
                else
                {
                    if (rand.Next(0, 2) == 0) // ���������� �ձ�
                    {
                        tile[y, x + 1] = TileType.Empty;
                    }
                    else // �Ʒ��� �ձ�
                    {
                        int carvePoint = rand.Next(runStart, x + 1);
                        tile[y + 1, carvePoint] = TileType.Empty;
                        runStart = x + 2;
                    }
                }
            }
        }
    }

    void DrawMaze()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector3 position = new Vector3(x, 0, y);
                if (tile[y, x] == TileType.Wall)
                {
                    // �� ����
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    // �ٴ� ����
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    void EnsurePath()
    {
        // ���� �������� ���� ���������� ������ ����
        bool[,] visited = new bool[size, size];
        if (!DFS(1, 1, visited))
        {
            Debug.Log("�������� ������ ������ ����. ���� ���� ��...");
            ConnectPath(1, 1, size - 2, size - 2);
        }
    }

    bool DFS(int x, int y, bool[,] visited)
    {
        if (x == size - 2 && y == size - 2) // ���� ������ ����
            return true;

        visited[y, x] = true;

        // �����¿� �̵�
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx > 0 && nx < size && ny > 0 && ny < size && tile[ny, nx] == TileType.Empty && !visited[ny, nx])
            {
                if (DFS(nx, ny, visited))
                    return true;
            }
        }

        return false;
    }

    void ConnectPath(int sx, int sy, int ex, int ey)
    {
        // �ܼ��� ��� ���� (��������)
        while (sx != ex)
        {
            tile[sy, sx] = TileType.Empty;
            sx += (ex > sx) ? 1 : -1;
        }

        while (sy != ey)
        {
            tile[sy, sx] = TileType.Empty;
            sy += (ey > sy) ? 1 : -1;
        }
    }

    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            // ���� ������ ���� ������ ����
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform);
            Debug.Log($"���� ������ �����Ǿ����ϴ�: {exitPosition}");
        }
        else
        {
            Debug.LogError("ExitPrefab�� �������� �ʾҽ��ϴ�!");
        }
    }

    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            // XR Rig�� ���������� �̵�
            xrRig.transform.position = startPosition;
            Debug.Log($"XR Rig�� ���� �������� �̵��Ǿ����ϴ�: {startPosition}");
        }
        else
        {
            Debug.LogError("XR Rig�� �������� �ʾҽ��ϴ�!");
        }
    }
}
