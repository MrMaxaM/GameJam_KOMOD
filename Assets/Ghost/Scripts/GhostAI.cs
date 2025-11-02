using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class GhostAI : MonoBehaviour
{
    public Transform targetPoint;
    public float maxDistance = 3f;
    public float minDistance = 2f;
    public float arrivalThreshold = 0.5f; // Дистанция для "достижения" точки

    private NavMeshAgent agent;
    private Transform player;
    private bool hasReachedTarget = false;
    
    public UnityEvent OnGuideCompleted;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (hasReachedTarget) return;
        
        // Проверяем достигли ли целевой точки
        if (Vector3.Distance(transform.position, targetPoint.position) <= arrivalThreshold)
        {
            CompleteGuide();
            return;
        }
        
        UpdateMovement();
    }

    void UpdateMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 targetPosition = transform.position;

        if (distanceToPlayer < minDistance)
        {
            targetPosition = targetPoint.position;
        }
        else if (distanceToPlayer > maxDistance)
        {
            targetPosition = player.position;
        }
        
        agent.SetDestination(targetPosition);
    }

    void CompleteGuide()
    {
        hasReachedTarget = true;
        agent.isStopped = true;

        OnGuideCompleted?.Invoke();
        
        Destroy(gameObject, 0.1f);
        
        Debug.Log("Проводник довёл игрока!");
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        if (targetPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPoint.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPoint.position, 0.3f);
        }
        
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}