using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float waitTimeAtPoint = 2f;
    public float sightWaitTime = 3f;
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public BoxCollider2D wanderArea;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;

    [Header("Debug Info")]
    [SerializeField] private float stateTimer;
    [SerializeField] private float attackTimer;
    [SerializeField] private Vector3 lastKnownPlayerPosition;
    [SerializeField] private AIState currentState = AIState.Wandering;

    private NavMeshAgent agent;
    private Transform player;
    private enum AIState { Wandering, Chasing, Attacking, Searching }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = wanderSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        SetWanderDestination();
    }

    void Update()
    {
        UpdateTimers();
        
        switch (currentState)
        {
            case AIState.Wandering:
                UpdateWandering();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.Attacking:
                UpdateAttacking();
                break;
            case AIState.Searching:
                UpdateSearching();
                break;
        }
    }

    void UpdateTimers()
    {
        stateTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
    }

    void UpdateWandering()
    {
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance && stateTimer <= 0)
        {
            SetWanderDestination();
        }
    }

    void UpdateChasing()
    {
        if (!CanSeePlayer())
        {
            StartSearching();
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            StartAttacking();
            return;
        }
        
        agent.SetDestination(player.position);
        lastKnownPlayerPosition = player.position;
    }

    void UpdateAttacking()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            StartChasing();
            return;
        }
        
        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    void UpdateSearching()
    {
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }

        if (stateTimer > 0) return;
        
        StartWandering();
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;
        
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            player.position - transform.position, 
            detectionRange,
            LayerMask.GetMask("Walls", "Player")
        );
        
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    void SetWanderDestination()
    {
        Vector3 randomPoint;

        if (wanderArea != null)
        {
            Bounds bounds = wanderArea.bounds;
        
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);

            randomPoint = new Vector3(randomX, randomY, 0);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                stateTimer = waitTimeAtPoint;
            }
        }    
    }

    void StartWandering()
    {
        currentState = AIState.Wandering;
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        SetWanderDestination();
    }

    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
    }

    void StartAttacking()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
        attackTimer = 0;
    }

    void StartSearching()
    {
        currentState = AIState.Searching;
        agent.SetDestination(lastKnownPlayerPosition);
        stateTimer = sightWaitTime;
        agent.isStopped = false;
    }

    void PerformAttack()
    {
        Debug.Log($"Монстр атаковал игрока!");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.TakeDamage();
        Debug.Log($"Монстр атаковал игрока!");
    }

    void OnDrawGizmosSelected()
    {
        // Зона блуждания
        if (wanderArea != null)
        {
            Gizmos.color = Color.blue;
            Bounds bounds = wanderArea.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        
        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Направление к игроку
        if (currentState == AIState.Chasing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}