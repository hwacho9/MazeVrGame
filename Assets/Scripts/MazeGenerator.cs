using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;  // 미로의 크기 (홀수)
    public GameObject wallPrefab;  // 벽 프리팹
    public GameObject floorPrefab; // 바닥 프리팹
    public GameObject exitPrefab;  // 종료 지점 프리팹
    public GameObject xrRig;       // XR Rig (플레이어)

    private TileType[,] tile; // 미로 배열
    private Vector3 startPosition; // 시작 지점
    private Vector3 exitPosition;  // 종료 지점

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size); // 미로 생성
        DrawMaze();       // 미로 그리기
        EnsurePath();     // 시작점에서 종료점까지 경로 보장
        PlaceExit();      // 종료 지점 생성
        PlaceXRRig();     // XR Rig를 시작점으로 이동
    }

    public void Initialize(int size)
    {
        if (size % 2 == 0)
        {
            Debug.LogError("미로 크기는 홀수여야 합니다.");
            return;
        }

        tile = new TileType[size, size];

        // SideWinder 알고리즘 사용
        GenerateBySideWinder();

        // 시작 지점과 종료 지점 설정
        startPosition = new Vector3(1, 0, 1); // 시작 지점 (x, y, z)
        exitPosition = new Vector3(size - 2, 0, size - 2); // 종료 지점 (x, y, z)
    }

    void GenerateBySideWinder()
    {
        // 모든 타일을 벽으로 초기화
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

        // SideWinder 알고리즘으로 길을 생성
        System.Random rand = new System.Random();
        for (int y = 1; y < size; y += 2)
        {
            int runStart = 1; // 오른쪽으로 연결되는 시작 위치
            for (int x = 1; x < size; x += 2)
            {
                // 맨 마지막 열
                if (x == size - 2)
                {
                    if (y < size - 2) // 아래로 연결
                        tile[y + 1, x] = TileType.Empty;
                }
                // 맨 마지막 행
                else if (y == size - 2)
                {
                    tile[y, x + 1] = TileType.Empty; // 오른쪽으로 연결
                }
                else
                {
                    if (rand.Next(0, 2) == 0) // 오른쪽으로 뚫기
                    {
                        tile[y, x + 1] = TileType.Empty;
                    }
                    else // 아래로 뚫기
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
                    // 벽 생성
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    // 바닥 생성
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    void EnsurePath()
    {
        // 시작 지점에서 종료 지점까지의 연결을 보장
        bool[,] visited = new bool[size, size];
        if (!DFS(1, 1, visited))
        {
            Debug.Log("시작점과 종료점 연결이 없음. 연결 생성 중...");
            ConnectPath(1, 1, size - 2, size - 2);
        }
    }

    bool DFS(int x, int y, bool[,] visited)
    {
        if (x == size - 2 && y == size - 2) // 종료 지점에 도달
            return true;

        visited[y, x] = true;

        // 상하좌우 이동
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
        // 단순한 경로 연결 (직선으로)
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
            // 종료 지점에 종료 프리팹 생성
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform);
            Debug.Log($"종료 지점이 생성되었습니다: {exitPosition}");
        }
        else
        {
            Debug.LogError("ExitPrefab이 설정되지 않았습니다!");
        }
    }

    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            // XR Rig를 시작점으로 이동
            xrRig.transform.position = startPosition;
            Debug.Log($"XR Rig가 시작 지점으로 이동되었습니다: {startPosition}");
        }
        else
        {
            Debug.LogError("XR Rig가 설정되지 않았습니다!");
        }
    }
}
