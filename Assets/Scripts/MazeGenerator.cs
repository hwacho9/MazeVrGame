using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;                // 미로의 크기 (홀수 권장)
    public GameObject wallPrefab;        // 벽 프리팹
    public GameObject floorPrefab;       // 바닥 프리팹
    public GameObject exitPrefab;        // 종료 지점 프리팹
    public GameObject xrRig;             // XR Rig (플레이어)
    public RawImage miniMapImage;        // 미니맵을 표시할 UI 이미지
    public EnemyManager enemyManager;    // EnemyManager 참조
    public TextMeshProUGUI endGameText;             // 종료 메시지 UI
    public Button restartButton;         // 재시작 버튼 UI

    private TileType[,] tile;            // 미로 배열 (Wall, Empty)
    private Vector3 startPosition;       // 시작 지점
    private Vector3 exitPosition;        // 종료 지점
    private Texture2D miniMapTexture;    // 미니맵 텍스처
    private Vector2Int lastPlayerPosition; // 이전 플레이어 위치

    private Color lastPlayerTileColor;   // 이전 플레이어 위치의 색상 저장 변수

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
        enemyManager.SpawnEnemies(0, tile, size, transform);

        // UI 초기화
        endGameText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // 매 프레임마다 미니맵에서 플레이어 위치 갱신
        CheckExitReached();      // 종료 지점 도달 여부 확인
    }

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

        Debug.Log($"플레이어 위치: {playerPosition}, 종료 지점: {exitPosition}, 거리: {distance}");

        if (distance < 0.5f)
        {
            Debug.Log("플레이어가 종료 지점에 도착했습니다!"); // 로그 출력
            EndGame();
        }
        else
        {
            Debug.Log("플레이어가 아직 종료 지점에 도달하지 않았습니다.");
        }
    }


    void EndGame()
    {
        // 종료 메시지 및 재시작 버튼 활성화
        if (endGameText != null && restartButton != null)
        {
            endGameText.text = "탈출 성공! 게임 종료!";
            endGameText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);

            // 게임 멈춤
            Time.timeScale = 0;

            Debug.Log("게임 종료 화면이 표시되었습니다.");
        }
        else
        {
            Debug.LogError("endGameText 또는 restartButton이 설정되지 않았습니다. UI를 확인하세요.");
        }
    }


    public void RestartGame()
    {
        // 기존 맵 제거
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 미니맵 텍스처 초기화
        if (miniMapTexture != null)
        {
            Destroy(miniMapTexture);
        }

        // Time.timeScale을 다시 1로 설정
        Time.timeScale = 1;

        // 종료 메시지와 버튼 숨김
        if (endGameText != null)
        {
            endGameText.gameObject.SetActive(false);
        }
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        // 미로 재생성
        Initialize(size);    // 미로 배열 초기화
        DrawMaze();          // 미로 그리기
        EnsurePath();        // 경로 확인
        PlaceExit();         // 종료 지점 설정
        PlaceXRRig();        // 플레이어 시작 위치로 이동
        GenerateMiniMap();   // 미니맵 재생성

        // 적 재생성
        enemyManager.SpawnEnemies(1, tile, size, transform);

        Debug.Log("맵과 게임이 재시작되었습니다!");
    }

}
