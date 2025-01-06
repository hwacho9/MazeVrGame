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
    public GameObject xrRig;            // XR Rig (플레이어)
    public RawImage miniMapImage;        // 미니맵을 표시할 UI 이미지

    private TileType[,] tile;            // 미로 배열 (Wall, Empty)
    private Vector3 startPosition;       // 시작 지점
    private Vector3 exitPosition;        // 종료 지점
    private Texture2D miniMapTexture;    // 미니맵 텍스처
    private Vector2Int lastPlayerPosition; // 이전 플레이어 위치

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size);    // 미로 생성
        DrawMaze();          // 미로 그리기
        EnsurePath();        // 경로가 보장되도록 처리
        PlaceExit();         // 종료 지점 생성
        PlaceXRRig();        // XR Rig를 시작점으로 이동
        GenerateMiniMap();   // 미니맵 생성
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // 미니맵에서 플레이어 위치 갱신
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
                // 맨 첫 행이면 “서쪽”만 뚫을 수 없음
                // 맨 첫 열이면 “북쪽”만 뚫을 수 없음
                // 보통 Binary Tree에서는 "북쪽 혹은 동쪽"을 선택하는 버전도 있지만
                // 여기서는 "북쪽 혹은 서쪽"으로 예시를 들어볼게요.

                // 방향을 0 또는 1로 랜덤 결정 (0: 북, 1: 서)
                int direction = rand.Next(0, 2);

                // 만약 y == 1 이라면 북쪽으로 뚫을 수 없음 -> 서쪽 뚫기
                // 만약 x == 1 이라면 서쪽으로 뚫을 수 없음 -> 북쪽 뚫기
                // 둘 다 가능하면 랜덤으로

                if (y == 1 && x == 1)
                {
                    // (1,1)은 아무것도 뚫지 않거나
                    // 혹은 아래쪽/오른쪽은 이미 Empty로 설정되었으니 그대로 두어도 됨.
                }
                else if (y == 1) // 맨 위 줄이면 북쪽은 이미 범위 밖이므로 서쪽만 가능
                {
                    CarveWest(x, y);
                }
                else if (x == 1) // 맨 왼쪽 열이면 서쪽은 이미 범위 밖이므로 북쪽만 가능
                {
                    CarveNorth(x, y);
                }
                else
                {
                    if (direction == 0)
                    {
                        // 북쪽 뚫기
                        CarveNorth(x, y);
                    }
                    else
                    {
                        // 서쪽 뚫기
                        CarveWest(x, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 해당 셀(x, y)의 “북쪽 벽”을 허무는 함수
    /// </summary>
    void CarveNorth(int x, int y)
    {
        // y-1 위치가 벽이라면 뚫는다
        if (y - 1 >= 0 && tile[y - 1, x] == TileType.Wall)
        {
            tile[y - 1, x] = TileType.Empty;
        }
    }

    /// <summary>
    /// 해당 셀(x, y)의 “서쪽 벽”을 허무는 함수
    /// </summary>
    void CarveWest(int x, int y)
    {
        // x-1 위치가 벽이라면 뚫는다
        if (x - 1 >= 0 && tile[y, x - 1] == TileType.Wall)
        {
            tile[y, x - 1] = TileType.Empty;
        }
    }

    /// <summary>
    /// 미로를 실제 월드 오브젝트로 그려주는 함수
    /// </summary>
    void DrawMaze()
    {
        // 혹시 이전에 만들어진 자식 오브젝트가 있으면 정리
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
    /// 경로가 없으면 강제로 뚫어주는 함수
    /// </summary>
    void EnsurePath()
    {
        // BFS로 (1,1)에서 (size-2, size-2)로 갈 수 있는지 확인
        if (!CheckIfPathExists())
        {
            // 경로가 없다면 간단히 직선으로 뚫는 예시
            CarveDirectPath();
            // 새로 뚫었으니 다시 그려줌
            DrawMaze();
        }
    }

    /// <summary>
    /// BFS로 시작점~종료점 경로 존재 여부 체크
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
    /// 시작점~종료점 사이에 직선으로 길을 뚫는 예시
    /// </summary>
    void CarveDirectPath()
    {
        // (1,1)에서 오른쪽으로 쭉 뚫은 뒤, 아래로 쭉 뚫는다
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
    /// </summary>
    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition;
        }
    }

    /// <summary>
    /// 미니맵 텍스처 생성 및 UI 적용
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
                    miniMapTexture.SetPixel(x, y, Color.green); // 시작 지점
                else if (x == size - 2 && y == size - 2)
                    miniMapTexture.SetPixel(x, y, Color.red);   // 종료 지점
                else
                    miniMapTexture.SetPixel(x, y, Color.white);
            }
        }

        miniMapTexture.Apply();
        miniMapImage.texture = miniMapTexture;
    }

    /// <summary>
    /// 미니맵에서 플레이어의 위치를 매 프레임 갱신
    /// </summary>
    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // 현재 플레이어 위치
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // 이전 위치를 흰색으로 되돌림
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, Color.white);
        }

        // 현재 위치를 파란색으로 표시
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);
        miniMapTexture.Apply();

        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }
}
