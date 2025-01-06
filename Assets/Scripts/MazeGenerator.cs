using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;  // �̷��� ũ�� (Ȧ��)
    public GameObject wallPrefab;  // �� ������
    public GameObject floorPrefab; // �ٴ� ������
    public GameObject exitPrefab;  // ���� ���� ������
    public GameObject xrRig;       // XR Rig (�÷��̾�)
    public RawImage miniMapImage;  // �̴ϸ��� ǥ���� UI �̹���

    private TileType[,] tile; // �̷� �迭
    private Vector3 startPosition; // ���� ����
    private Vector3 exitPosition;  // ���� ����
    private Texture2D miniMapTexture; // �̴ϸ� �ؽ�ó
    private Vector2Int lastPlayerPosition; // ���� �÷��̾� ��ġ

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size); // �̷� ����
        DrawMaze();       // �̷� �׸���
        EnsurePath();     // ���������� ���������� ��� ����
        PlaceExit();      // ���� ���� ����
        PlaceXRRig();     // XR Rig�� ���������� �̵�
        GenerateMiniMap(); // �̴ϸ� ����
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // �� �����Ӹ��� �̴ϸʿ��� �÷��̾� ��ġ ������Ʈ
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
                    Instantiate(wallPrefab, position, Quaternion.identity, transform); // �� ����
                }
                else
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform); // �ٴ� ����
                }
            }
        }
    }

    void EnsurePath()
    {
        // ��� ���� ���� (����)
    }

    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform); // ���� ���� ����
        }
    }

    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition; // XR Rig�� ���������� �̵�
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
        miniMapImage.texture = miniMapTexture; // �̴ϸ� �ؽ�ó ����
    }

    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // ���� �÷��̾� ��ġ ���
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // ���� ��ġ �ʱ�ȭ
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, Color.white);
        }

        // ���ο� ��ġ�� �÷��̾� ���� ����
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);
        miniMapTexture.Apply();

        // ���� ��ġ ������Ʈ
        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }
}
