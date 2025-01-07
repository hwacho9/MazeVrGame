using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target; // 플레이어의 Transform
    private NavMeshAgent agent;
    private bool isDisabled = false; // 적이 무력화 상태인지 여부

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent가 설정되지 않았습니다.");
        }
    }

    void Update()
    {
        if (!isDisabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position); // 플레이어 추적
        }
        else if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}가 NavMesh를 벗어났습니다!");
        }
    }

    // 적 움직임을 비활성화
    public void DisableMovement(float duration)
    {
        if (isDisabled) return;

        isDisabled = true;
        agent.isStopped = true; // NavMeshAgent 멈춤

        // 적의 콜라이더를 비활성화하여 플레이어가 통과 가능
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        StartCoroutine(EnableMovementAfterDelay(duration));
    }

    private IEnumerator EnableMovementAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        isDisabled = false;
        agent.isStopped = false; // NavMeshAgent 재개

        // 적의 콜라이더를 다시 활성화
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        Debug.Log($"{gameObject.name}의 움직임이 재개되었습니다.");
    }
}

