using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target; // 플레이어의 Transform
    private NavMeshAgent agent;

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
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target.position); // 플레이어 추적
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}가 NavMesh를 벗어났습니다!");
        }
    }
}
