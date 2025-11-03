using UnityEngine;
using UnityEngine.AI;

public class RageAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float normalHearingRange = 8f;
    public float crouchHearingRange = 3f;
    public float wanderSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float waitTimeAtPoint = 2f;
    public float currentHearingRange;
    public BoxCollider2D wanderArea;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    public float chargeReadyTime = 0.5f;
    public float chargeDistance = 4f;
    public float chargeSpeed = 8f;
    public float chargeDuration = 0.5f;
    public float playerKnockbackForce = 5f;
    
    [Header("Destruction")]
    public GameObject destructionEffect;
    
    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController;
    private Vector3 lastHeardPosition;
    private float stateTimer;
    private float attackTimer;
    
    private enum AIState { Wandering, Chasing, PreparingCharge, Charging, Searching }
    private AIState currentState = AIState.Wandering;
    
    private Vector3 chargeDirection;
    private float chargeTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        currentHearingRange = normalHearingRange;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        
        SetWanderDestination();
    }

    void Update()
    {
        UpdateTimers();
        UpdateHearingRange();
        
        switch (currentState)
        {
            case AIState.Wandering:
                UpdateWandering();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.PreparingCharge:
                UpdatePreparingCharge();
                break;
            case AIState.Charging:
                UpdateCharging();
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
        
        if (currentState == AIState.Charging)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0)
            {
                EndCharge();
            }
        }
    }

    void UpdateHearingRange()
    {
        if (playerController != null)
        {
            currentHearingRange = playerController.isCrouching ? crouchHearingRange : normalHearingRange;
        }
    }

    void UpdateWandering()
    {
        if (CanHearPlayer())
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
        if (!CanHearPlayer())
        {
            StartSearching();
            return;
        }

        lastHeardPosition = player.position;
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= chargeDistance && attackTimer <= 0)
        {
            StartPreparingCharge();
        }
    }
    
    void UpdatePreparingCharge()
    {
        agent.isStopped = true;
        
        if (stateTimer <= 0)
        {
            StartCharging();
        }
    }

    void UpdateCharging()
    {
        if (currentState != AIState.Charging) return;
        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= 1f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
            
            // Отталкиваем игрока
            Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
            Rigidbody2D playerRb = player.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.AddForce(knockbackDirection * playerKnockbackForce, ForceMode2D.Impulse);
                EndCharge();
            }
        }
    }

    void UpdateSearching()
    {
        if (stateTimer > 0) return;
        
        if (CanHearPlayer())
        {
            StartChasing();
            return;
        }
        
        StartWandering();
    }

    bool CanHearPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= currentHearingRange;
    }

    void StartPreparingCharge()
    {
        currentState = AIState.PreparingCharge;
        agent.isStopped = true;
        stateTimer = chargeReadyTime;
        chargeDirection = (player.position - transform.position).normalized;

        Debug.Log("ГНЕВ: Готовлюсь к атаке!");
    }

    void StartCharging()
    {
        currentState = AIState.Charging;
        chargeDirection = (player.position - transform.position).normalized;
        chargeTimer = chargeDuration;

        agent.enabled = false;
        
        Debug.Log("ГНЕВ: Атакую!");
    }

    void EndCharge()
    {
        attackTimer = attackCooldown;
        
        agent.enabled = true;
        
        if (CanHearPlayer())
        {
            StartChasing();
        }
        else
        {
            StartSearching();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != AIState.Charging) return;
        
        DestructibleObject destructible = collision.gameObject.GetComponent<DestructibleObject>();
        if (destructible != null)
        {
            destructible.DestroyObject();

            if (destructionEffect != null)
            {
                Instantiate(destructionEffect, collision.contacts[0].point, Quaternion.identity);
            }
        }
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
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lastHeardPosition = player.position;
    }

    void StartSearching()
    {
        currentState = AIState.Searching;
        agent.SetDestination(lastHeardPosition);
        stateTimer = waitTimeAtPoint;
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        // Радиус слышимости
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentHearingRange);
        
        // Направление заряда
        if (currentState == AIState.Charging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + chargeDirection * 3f);
        }
        
        // Последняя услышанная позиция
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(lastHeardPosition, 0.3f);
    }
}