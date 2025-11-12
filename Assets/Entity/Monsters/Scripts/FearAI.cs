using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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
    public GameObject deathParticlesPrefab;
    public GameObject itemDropPrefab; 

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

    [Header("Audio Settings")]
    public AudioClip attackClip;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Transform player;
    private HideController hideController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D monsterCollider;
    private Rigidbody2D rb;
    private int currentWanderPointIndex = -1;
    private enum AIState { Wandering, Waiting, Chasing, Attacking, Searching, Dashing, Dying }
    private Animator animator;
    private Vector2 move;

    private AdaptiveMusicManager musicManager;
    private CameraFollow cameraEffects;
    private float distanceToPlayer;

    void Start()
    {
        cameraEffects = Camera.main.GetComponent<CameraFollow>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = wanderSpeed;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        hideController = player.GetComponent<HideController>();
        rb = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        musicManager = FindFirstObjectByType<AdaptiveMusicManager>(); // ищем глобальный менеджер музыки

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Debug.Log("Тест1");
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        SetWanderDestination();
    }

    void Update()
    {
        UpdateTimers();

        move = agent.velocity;

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
        
        // Обновляем последнее направление если есть движение
        if (move.magnitude > 0.1f)
        {
            animator.SetFloat("LastX", move.x);
        }
        
        // Устанавливаем параметры движения
        animator.SetFloat("X", move.x);
        
        // Устанавливаем булевые параметры
        animator.SetBool("isWalking", move.magnitude > 0.1f);
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
        
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            StartAttacking();
            return;
        }
        
        agent.SetDestination(player.position);
        lastKnownPlayerPosition = player.position;

        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);
    }

    void UpdateAttacking()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Debug.Log($"{distanceToPlayer}, {attackRange}");
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
        
        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);
    }

    void UpdateSearching()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);

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
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

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
        
        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);
    }

    bool CanSeePlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
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
        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Calm);
    }
    
    void StartWandering()
    {
        currentState = AIState.Wandering;
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        ResetAppearance();
        SetWanderDestination();
        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Calm);

        cameraEffects.UpdateThreatEffect(distanceToPlayer, false);
    }

    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        ResetAppearance();

        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Chase);
    }

    void StartAttacking()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
        ResetAppearance();
        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Chase);
    }

    void StartSearching()
    {
        currentState = AIState.Searching;
        agent.SetDestination(lastKnownPlayerPosition);
        stateTimer = sightWaitTime;
        agent.isStopped = false;
        ResetAppearance();
        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Search);
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
        
        PlaySound(attackClip);

        dashTimer = dashDuration;
        ResetAppearance();
        if (musicManager != null)
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Chase);
    }

    public void UpdateLocation(string newLocation)
    {
        Debug.Log("Новая локация");
        if (musicManager != null && newLocation == "past")
            musicManager.Stop();
        else
            musicManager.Play();
            musicManager.SetState(AdaptiveMusicManager.MonsterState.Calm);
    }

    void PerformAttack()
    {
        Debug.Log($"Монстр атаковал игрока!");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.TakeDamage();
        Debug.Log($"Монстр атаковал игрока!");

        Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
        Rigidbody2D playerRb = player.gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
        }
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