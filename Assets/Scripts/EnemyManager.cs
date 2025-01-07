using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static MazeGenerator;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;  // 적 프리팹
    public GameObject xrRig;        // 플레이어 (XR Rig)
    public Text gameOverText;       // 게임 오버 메시지
    public Button restartButton;    // 재시작 버튼 UI

    private List<GameObject> enemies = new List<GameObject>();
    private bool isFlashbangActive = false; // 플래시뱅 활성화 여부

    /// <summary>
    /// 적 생성
    /// </summary>
    public void SpawnEnemies(int count, TileType[,] tile, int mazeSize, Transform mazeParent)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomEmptyPosition(tile, mazeSize);

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, mazeParent);

            // NavMeshAgent 컴포넌트 확인 및 초기화
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.speed = 3.0f; // 적 이동 속도 설정
                agent.isStopped = true; // 초기 상태에서 정지
            }
            else
            {
                Debug.LogError("NavMeshAgent가 적에 추가되지 않았거나 NavMesh 위에 배치되지 않았습니다!");
            }

            enemies.Add(enemy);
        }

        // 5초 후에 적 움직임 시작
        StartCoroutine(EnableEnemyMovement());
    }

    /// <summary>
    /// 적 움직임을 활성화하는 Coroutine
    /// </summary>
    private IEnumerator EnableEnemyMovement()
    {
        yield return new WaitForSeconds(5f); // 5초 대기

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false; // 적 움직임 활성화
            }
        }
        Debug.Log("모든 적의 움직임이 활성화되었습니다!");
    }

    public void ClearEnemies()
    {
        // 모든 적 제거
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        enemies.Clear(); // 리스트 초기화
    }

    /// <summary>
    /// 적 리스트에서 플레이어와의 충돌 확인
    /// </summary>
    void Update()
    {
        if (isFlashbangActive)
        {
            Debug.Log("플래시뱅 활성화 중: 적의 추적 및 충돌 중단");
            return; // 플래시뱅 활성화 중에는 충돌 검사 중단
        }

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isStopped)
            {
                // 플레이어 위치를 적의 목표로 설정
                agent.SetDestination(xrRig.transform.position);
            }

            // 적과 플레이어의 거리 계산
            float distance = Vector3.Distance(enemy.transform.position, xrRig.transform.position);

            if (distance < 0.5f) // 충돌 시
            {
                Debug.Log("플레이어가 적에게 잡혔습니다!"); // 로그 출력
                GameOver();
                break;
            }
            else
            {
                Debug.Log("플레이어가 아직 적에게 잡히지 않았습니다.");
            }
        }
    }


    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void GameOver()
    {
        // Game Over 메시지 표시
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over!";
            gameOverText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Text가 설정되지 않았습니다!");
        }

        // Restart 버튼 표시
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Restart Button이 설정되지 않았습니다!");
        }

        // 게임 멈춤
        Time.timeScale = 0;
    }

    /// <summary>
    /// 빈 타일에서 무작위 위치 반환
    /// </summary>
    private Vector3 GetRandomEmptyPosition(TileType[,] tile, int mazeSize)
    {
        int x, y;

        while (true)
        {
            x = Random.Range(1, mazeSize - 1);
            y = Random.Range(1, mazeSize - 1);

            if (tile[y, x] == TileType.Empty) // 빈 칸이면 위치 반환
            {
                return new Vector3(x, 0, y); // NavMesh Bake 높이에 맞게 Y값 조정
            }
        }
    }

    /// <summary>
    /// 플래시뱅 사용
    /// </summary>
    public void TriggerFlashbang()
    {
        isFlashbangActive = true;
        Debug.Log($"플래시뱅 활성화됨: {isFlashbangActive}");

        foreach (GameObject enemy in enemies)
        {
            Collider enemyCollider = enemy.GetComponent<Collider>();
            Collider playerCollider = xrRig.GetComponent<Collider>();

            if (enemyCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, enemyCollider, true); // 충돌 무시
            }

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                StartCoroutine(DisableEnemyTemporarily(agent, 5f)); // 5초 동안 멈춤
            }
        }

        // 5초 후 플래시뱅 비활성화
        StartCoroutine(EndFlashbang(5f));
    }


    private IEnumerator EndFlashbang(float duration)
    {
        yield return new WaitForSeconds(duration);
        isFlashbangActive = false;
        Debug.Log("플래시뱅 비활성화됨: " + isFlashbangActive);
    }


    private IEnumerator DisableEnemyTemporarily(NavMeshAgent agent, float duration)
    {
        agent.isStopped = true; // 적 멈춤
        yield return new WaitForSeconds(duration); // 대기
        agent.isStopped = false; // 적 다시 움직임
    }
}
