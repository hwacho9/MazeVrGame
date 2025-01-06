using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int size = 11;  // 미로의 크기 (홀수)
    public GameObject wallPrefab;  // 벽 프리팹
    public GameObject floorPrefab; // 바닥 프리팹
    public GameObject exitPrefab;  // 종료 지점 프리팹
    public GameObject xrRig;       // XR Rig (플레이어)
    public RawImage miniMapImage;  // 미니맵을 표시할 UI 이미지

    private TileType[,] tile; // 미로 배열
    private Vector3 startPosition; // 시작 지점
    private Vector3 exitPosition;  // 종료 지점
    private Texture2D miniMapTexture; // 미니맵 텍스처
    private Vector2Int lastPlayerPosition; // 이전 플레이어 위치

    private enum TileType { Wall, Empty };

    void Start()
    {
        Initialize(size); // 미로 생성
        DrawMaze();       // 미로 그리기
        EnsurePath();     // 시작점에서 종료점까지 경로 보장
        PlaceExit();      // 종료 지점 생성
        PlaceXRRig();     // XR Rig를 시작점으로 이동
        GenerateMiniMap(); // 미니맵 생성
    }

    void Update()
    {
        UpdatePlayerOnMiniMap(); // 매 프레임마다 미니맵에서 플레이어 위치 업데이트
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
                    Instantiate(wallPrefab, position, Quaternion.identity, transform); // 벽 생성
                }
                else
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform); // 바닥 생성
                }
            }
        }
    }

    void EnsurePath()
    {
        // 경로 연결 보장 (생략)
    }

    void PlaceExit()
    {
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, exitPosition, Quaternion.identity, transform); // 종료 지점 생성
        }
    }

    void PlaceXRRig()
    {
        if (xrRig != null)
        {
            xrRig.transform.position = startPosition; // XR Rig를 시작점으로 이동
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
                    miniMapTexture.SetPixel(x, y, Color.green); // 시작 지점
                else if (x == size - 2 && y == size - 2)
                    miniMapTexture.SetPixel(x, y, Color.red);   // 종료 지점
                else
                    miniMapTexture.SetPixel(x, y, Color.white);
            }
        }

        miniMapTexture.Apply();
        miniMapImage.texture = miniMapTexture; // 미니맵 텍스처 적용
    }

    void UpdatePlayerOnMiniMap()
    {
        if (xrRig == null || miniMapTexture == null) return;

        // 현재 플레이어 위치 계산
        Vector3 playerPosition = xrRig.transform.position;
        int playerX = Mathf.Clamp(Mathf.RoundToInt(playerPosition.x), 0, size - 1);
        int playerY = Mathf.Clamp(Mathf.RoundToInt(playerPosition.z), 0, size - 1);

        // 이전 위치 초기화
        if (lastPlayerPosition != Vector2Int.zero)
        {
            miniMapTexture.SetPixel(lastPlayerPosition.x, lastPlayerPosition.y, Color.white);
        }

        // 새로운 위치에 플레이어 색상 설정
        miniMapTexture.SetPixel(playerX, playerY, Color.blue);
        miniMapTexture.Apply();

        // 이전 위치 업데이트
        lastPlayerPosition = new Vector2Int(playerX, playerY);
    }
}
