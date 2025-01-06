using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target; // �÷��̾��� Transform
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent�� �������� �ʾҽ��ϴ�.");
        }
    }

    void Update()
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target.position); // �÷��̾� ����
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}�� NavMesh�� ������ϴ�!");
        }
    }
}
