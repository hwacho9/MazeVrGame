using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flashbang : MonoBehaviour
{
    public float flashDuration = 5f; // 플래시뱅 효과 지속 시간
    public float flashRange = 20f;   // 플래시뱅 효과 범위
    public LayerMask enemyLayer;    // 적 레이어 지정
    public EnemyManager enemyManager; // EnemyManager 참조

    private InputAction flashbangAction;

    void Start()
    {
        // Input Action 초기화
        var actionMap = GetComponent<PlayerInput>().actions;
        flashbangAction = actionMap["Flashbang"]; // Input Action의 Flashbang 이름

        // 버튼 눌렀을 때 실행
        flashbangAction.performed += _ => UseFlashbang();
    }

    public void UseFlashbang()
    {
        Debug.Log("플래시뱅 사용!");

        // EnemyManager의 플래시뱅 호출
        if (enemyManager != null)
        {
            enemyManager.TriggerFlashbang(); // TriggerFlashbang 호출
        }
        else
        {
            Debug.LogError("EnemyManager가 설정되지 않았습니다!");
        }

        StartCoroutine(DisableEnemies());
    }

    private IEnumerator DisableEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, flashRange, enemyLayer);

        if (enemies.Length > 0)
        {
            Debug.Log($"플래시뱅 효과 적용 대상: {enemies.Length}명");
        }
        else
        {
            Debug.LogWarning("플래시뱅 효과 범위에 적이 없습니다.");
        }

        foreach (Collider enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.DisableMovement(flashDuration);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        Debug.Log("플래시뱅 효과 종료!");
    }
}
