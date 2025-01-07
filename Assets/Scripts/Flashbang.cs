using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Flashbang : MonoBehaviour
{
    public int flashbangCount = 3; // 초기 플래시뱅 개수
    public float flashDuration = 5f; // 플래시뱅 효과 지속 시간
    public float flashRange = 20f;   // 플래시뱅 효과 범위
    public LayerMask enemyLayer;    // 적 레이어 지정
    public TextMeshProUGUI flashbangText;      // 플래시뱅 개수를 표시할 UI 텍스트

    private InputAction flashbangAction;

    void Start()
    {
        // Input Action 초기화
        var actionMap = GetComponent<PlayerInput>().actions;
        flashbangAction = actionMap["Flashbang"]; // Input Action의 Flashbang 이름

        // 버튼 눌렀을 때 실행
        flashbangAction.performed += _ => UseFlashbang();

        // 초기 플래시뱅 UI 업데이트
        UpdateFlashbangUI();
    }

    private void UpdateFlashbangUI()
    {
        if (flashbangText != null)
        {
            flashbangText.text = $"{flashbangCount}";
        }
        else
        {
            Debug.LogWarning("플래시뱅 UI 텍스트가 설정되지 않았습니다!");
        }
    }

    public void UseFlashbang()
    {
        if (flashbangCount > 0)
        {
            flashbangCount--; // 플래시뱅 개수 감소
            UpdateFlashbangUI(); // UI 업데이트

            Debug.Log("플래시뱅 사용!");
            StartCoroutine(DisableEnemies());
        }
        else
        {
            Debug.LogWarning("플래시뱅이 없습니다!");
        }
    }

    private IEnumerator DisableEnemies()
    {
        // 플레이어 주변의 적 탐지
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

    private void OnDestroy()
    {
        // 액션 정리
        if (flashbangAction != null)
        {
            flashbangAction.performed -= _ => UseFlashbang();
        }
    }
}
