using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class FearAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float waitTimeAtPoint = 2f;
    public float sightWaitTime = 3f;
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public Transform[] wanderPoints;
    public float hidingTime = 18f;
    public float dashDistance = 3f;
    public float dashDuration = 0.5f;

    [Header("Visual Settings")]
    public float darkenIntensity = 0.5f;
    public float transparency = 0.7f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;

    [Header("Debug Info")]
    [SerializeField] private float stateTimer;
    [SerializeField] private float attackTimer;
    [SerializeField] private Vector3 lastKnownPlayerPosition;
    [SerializeField] private AIState currentState = AIState.Wandering;
    [SerializeField] private float dashTimer;
    [SerializeField] private Vector3 dashTarget;

    private NavMeshAgent agent;
    private Transform player;
    private HideController hideController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private int currentWanderPointIndex = -1;
    private enum AIState { Wandering, Waiting, Chasing, Attacking, Searching, Dashing }

    private PlaylistManager playlistManager;

    public AudioClip attackClip;           // звук атаки

    private AudioSource audioSource;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = wanderSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hideController = GameObject.FindGameObjectWithTag("Player").GetComponent<HideController>();
        rb = GetComponent<Rigidbody2D>();
        playlistManager = FindFirstObjectByType<PlaylistManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

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
            case AIState.Waiting:
                UpdateWaiting();
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
            case AIState.Dashing:
                UpdateDashing();
                break;
        }
    }

    void UpdateTimers()
    {
        stateTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
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
            StartWaiting();
        }
    }
    
    void UpdateWaiting()
    {
        if (CanSeePlayer())
        {
            StartDashing();
            return;
        }
        
        if (stateTimer <= 0)
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

    void UpdateDashing()
    {
        if (dashTimer <= 0)
        {
            StartChasing();
            return;
        }
        
        float dashProgress = 1f - (dashTimer / dashDuration);
        transform.position = Vector3.Lerp(transform.position, dashTarget, dashProgress);
        
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            StartAttacking();
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

    void SetWanderDestination()
    {
        if (wanderPoints == null || wanderPoints.Length == 0)
        {
            Debug.LogError("Нет точек ожидания!");
            return;
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, wanderPoints.Length);
        } while (newIndex == currentWanderPointIndex && wanderPoints.Length > 1);
        
        currentWanderPointIndex = newIndex;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(wanderPoints[currentWanderPointIndex].position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            stateTimer = waitTimeAtPoint;
            currentState = AIState.Wandering;
            agent.speed = wanderSpeed;
            agent.isStopped = false;
            ResetAppearance();
        }
    }

    void StartWaiting()
    {
        currentState = AIState.Waiting;
        agent.isStopped = true;
        stateTimer = hidingTime;
        DarkenAppearance();
        playlistManager.PlayPlaylist("FearWaiting");
    }
    
    void StartWandering()
    {
        currentState = AIState.Wandering;
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        ResetAppearance();
        SetWanderDestination();
        playlistManager.PlayPlaylist("FearWandering");
    }

    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        ResetAppearance();
        playlistManager.PlayPlaylist("FearChasing");
    }

    void StartAttacking()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
        attackTimer = 0;
        ResetAppearance();
        PlaySound(attackClip);

        if (playlistManager != null)
            playlistManager.PlayPlaylist("FearChasing");
    }

    void StartSearching()
    {
        currentState = AIState.Searching;
        agent.SetDestination(lastKnownPlayerPosition);
        stateTimer = sightWaitTime;
        agent.isStopped = false;
        ResetAppearance();
    }

        void StartDashing()
    {
        currentState = AIState.Dashing;
        agent.isStopped = true;
        
        // Вычисляем направление и цель рывка
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        dashTarget = transform.position + directionToPlayer * dashDistance;
        
        // Убеждаемся, что точка рывка доступна на NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dashTarget, out hit, 2f, NavMesh.AllAreas))
        {
            dashTarget = hit.position;
        }
        
        dashTimer = dashDuration;
        ResetAppearance();
    }

    void PerformAttack()
    {
        Debug.Log($"Монстр атаковал игрока!");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.TakeDamage();
        Debug.Log($"Монстр атаковал игрока!");
    }

    void DarkenAppearance()
    {
        if (spriteRenderer != null)
        {
            Color darkenedColor = originalColor * darkenIntensity;
            darkenedColor.a = transparency;
            spriteRenderer.color = darkenedColor;
        }
    }

    void ResetAppearance()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnDrawGizmosSelected()
    {
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

    //

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}