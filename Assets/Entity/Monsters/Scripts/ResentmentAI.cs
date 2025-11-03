using UnityEngine;
using UnityEngine.AI;

public class ResentmentAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float returnToPuddleDistance = 10f;
    public float chaseSpeed = 3.5f;
    public BoxCollider2D wanderArea;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;

    [Header("Debug Info")]
    [SerializeField] private float attackTimer;
    [SerializeField] private AIState currentState = AIState.Idle;
    [SerializeField] private float stateTimer;
    [SerializeField] private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;
    private Transform player;
    
    private HideController hideController;
    private PuddleController homePuddle;
    private Vector3 puddlePosition;
    private enum AIState { Idle, Chasing, Attacking, Returning, Searching }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = chaseSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hideController = GameObject.FindGameObjectWithTag("Player").GetComponent<HideController>();
    }

    void Update()
    {
        UpdateTimers();

        switch (currentState)
        {
            case AIState.Idle:
                StartChasing();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.Attacking:
                UpdateAttacking();
                break;
            case AIState.Returning:
                UpdateReturning();
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
        
        // Проверяем, не слишком ли далеко от лужи
        float distanceToPuddle = Vector3.Distance(transform.position, puddlePosition);
        if (distanceToPuddle > returnToPuddleDistance)
        {
            StartReturning();
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

        // Проверяем, не слишком ли далеко от лужи
        float distanceToPuddle = Vector3.Distance(transform.position, puddlePosition);
        if (distanceToPuddle > returnToPuddleDistance)
        {
            StartReturning();
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

        // Проверяем, не слишком ли далеко от лужи
        float distanceToPuddle = Vector3.Distance(transform.position, puddlePosition);
        if (distanceToPuddle > returnToPuddleDistance)
        {
            StartReturning();
            return;
        }

        if (stateTimer > 0) return;
        
        StartReturning();
    }
    
    void UpdateReturning()
    {
        float distanceToPuddle = Vector3.Distance(transform.position, puddlePosition);
        
        if (distanceToPuddle <= 1.5f)
        {
            DisappearIntoPuddle();
            return;
        }
        
        if (CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= returnToPuddleDistance * 0.7f)
            {
                StartChasing();
            }
        }
    }
    
    
    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange || hideController.isHiding) return false;
        
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            player.position - transform.position, 
            detectionRange,
            LayerMask.GetMask("Walls", "Player")
        );
        
        return hit.collider != null && hit.collider.CompareTag("Player");
    }
    
    void StartChasing()
    {
        currentState = AIState.Chasing;
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
        stateTimer = 2f;
        agent.isStopped = false;
    }
    
    void StartReturning()
    {
        currentState = AIState.Returning;
        agent.isStopped = false;
        agent.SetDestination(puddlePosition);
    }
    
    void PerformAttack()
    {
        Debug.Log("Обида атаковала игрока!");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
        }
    }
    
    void DisappearIntoPuddle()
    {
        if (homePuddle != null)
        {
            homePuddle.ReturnMonster();
        }
        
        // Анимация исчезновения
        Destroy(gameObject);
    }
    
    public void SetHomePuddle(PuddleController puddle)
    {
        homePuddle = puddle;
        puddlePosition = puddle.transform.position;
    }
    
    void OnDrawGizmosSelected()
    {
        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Радиус возвращения к луже
        if (homePuddle != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puddlePosition, returnToPuddleDistance);
        }
        
        // Направление к игроку
        if (currentState == AIState.Chasing && player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // Направление к луже
        if (currentState == AIState.Returning && homePuddle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, puddlePosition);
        }
    }
}