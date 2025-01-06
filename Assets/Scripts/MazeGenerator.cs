using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;                // 미로의 크기 (홀수 권장)
    public GameObject wallPrefab;        // 벽 프리팹
    public GameObject floorPrefab;       // 바닥 프리팹
    public GameObject exitPrefab;        // 종료 지점 프리팹
    public GameObject xrRig;             // XR Rig (플레이어)
    public RawImage miniMapImage;        // 미니맵을 표시할 UI 이미지
    public EnemyManager enemyManager;    // EnemyManager 참조

    private TileType[,] tile;            // 미로 배열 (Wall, Empty)
    private Vector3 startPosition;       // 시작 지점
    private Vector3 exitPosition;        // 종료 지점
    private Texture2D miniMapTexture;    // 미니맵 텍스처
    private Vector2Int lastPlayerPosition; // 이전 플레이어 위치

    public enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size);    // 미로 생성
        DrawMaze();          // 미로 그리기
        EnsurePath();        // 경로가 보장되도록 처리
        PlaceExit();         // 종료 지점 생성
        PlaceXRRig();        // XR Rig를 시작점으로 이동
        GenerateMiniMap();   // 미니맵 생성

        // 적 생성 요청
        enemyManager.SpawnEnemies(3, tile, size, transform);
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // 매 프레임마다 미니맵에서 플레이어 위치 갱신
    }

    /// <summary>
    /// 미로 초기화
    /// </summary>
    public void Initialize(int size)
    {
        if (size % 2 == 0)
        {
            Debug.LogError("미로 크기는 홀수여야 합니다.");
            return;
        }

        tile = new TileType[size, size];

        // Binary Tree 알고리즘으로 미로 생성
        GenerateByBinaryTree();

        // 시작 지점과 종료 지점 설정
        startPosition = new Vector3(1, 0, 1);                 // (1,1)
        exitPosition = new Vector3(size - 2, 0, size - 2);   // (size-2, size-2)
    }

    /// <summary>
    /// Binary Tree 알고리즘으로 미로 생성
    /// </summary>
    void GenerateByBinaryTree()
    {
        // 1) 모든 셀을 일단 벽으로 초기화
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tile[y, x] = TileType.Wall;
            }
        }

        // 2) 내부(홀수 좌표)에 대해서 Empty로 만들기
        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                tile[y, x] = TileType.Empty;
            }
        }

        // 3) Binary Tree 규칙 적용: 각 (홀수, 홀수) 셀마다 북쪽 or 서쪽 벽 허물기
        System.Random rand = new System.Random();

        for (int y = 1; y < size; y += 2)
        {
            for (int x = 1; x < size; x += 2)
            {
                if (y == 1 && x == 1)
                {
                    continue; // (1,1)은 아무 것도 뚫지 않음
                }
                else if (y == 1) // 맨 위 줄이면 서쪽만 뚫기
                {
                    CarveWest(x, y);
                }
                else if (x == 1) // 맨 왼쪽 열이면 북쪽만 뚫기
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
    /// 북쪽 벽 허물기
    /// </summary>
    void CarveNorth(int x, int y)
    {
        if (y - 1 >= 0 && tile[y - 1, x] == TileType.Wall)
        {
            tile[y - 1, x] = TileType.Empty;
        }
    }

    /// <summary>
    /// 서쪽 벽 허물기
    /// </summary>
    void CarveWest(int x, int y)
    {
        if (x - 1 >= 0 && tile[y, x - 1] == TileType.Wall)
        {
            tile[y, x - 1] = TileType.Empty;
        }
    }

    /// <summary>
    /// 경로 보장 (BFS로 시작점~종료점 연결 확인)
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
    /// 종료 지점 생성
    /// </summary>
    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// 플레이어(XR Rig) 시작위치로 이동
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
        int pixelScale = 1; // 1 타일당 픽셀 크기 (확장 비율)
        int mapSize = size * pixelScale; // 텍스처 크기 = 미로 크기 * 픽셀 크기

        // 텍스처 초기화
        miniMapTexture = new Texture2D(mapSize, mapSize);
        miniMapTexture.filterMode = FilterMode.Point;
        miniMapTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 색상 결정: 벽, 시작 지점, 종료 지점, 경로
                Color color = tile[y, x] == TileType.Wall ? Color.black : Color.white;
                if (x == 1 && y == 1) color = Color.green; // 시작 지점
                if (x == size - 2 && y == size - 2) color = Color.red; // 종료 지점

                // 픽셀 확장: 한 타일을 여러 픽셀로 채움
                for (int dy = 0; dy < pixelScale; dy++)
                {
                    for (int dx = 0; dx < pixelScale; dx++)
                    {
                        miniMapTexture.SetPixel(x * pixelScale + dx, y * pixelScale + dy, color);
                    }
                }
            }
        }

        miniMapTexture.Apply(); // 텍스처 적용
        miniMapImage.texture = miniMapTexture; // UI 이미지에 텍스처 적용
    }


    /// <summary>
    /// 미니맵에서 플레이어의 위치를 매 프레임 갱신
    /// </summary>
    private Color lastPlayerTileColor; // 이전 플레이어 위치의 색상 저장 변수

    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // 현재 플레이어 위치를 미로의 좌표로 변환
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // 이전 위치를 원래 색상으로 되돌림
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerTileColor);
        }

        // 현재 위치의 기존 색상을 저장
        lastPlayerTileColor = miniMapTexture.GetPixel(playerX, playerY);

        // 현재 위치를 파란색으로 표시
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);

        miniMapTexture.Apply(); // 텍스처 갱신

        // 이전 위치 업데이트
        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }


}