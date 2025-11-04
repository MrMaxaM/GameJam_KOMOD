using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = wanderSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hideController = GameObject.FindGameObjectWithTag("Player").GetComponent<HideController>();
        rb = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        
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
    }
    
    void StartWandering()
    {
        currentState = AIState.Wandering;
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        ResetAppearance();
        SetWanderDestination();
    }

    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        ResetAppearance();
    }

    void StartAttacking()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
        attackTimer = 0;
        ResetAppearance();
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

    public void StartDying()
    {
        Debug.Log($"СМЭРТЬ!");
        currentState = AIState.Dying;
        agent.isStopped = true;
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        // Отключаем физику и коллайдер
        if (monsterCollider != null)
            monsterCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Отключаем физическое воздействие
        }
        
        yield return new WaitForSeconds(0.4f);

        // Спавним партиклы смерти
        if (deathParticlesPrefab != null)
        {
            GameObject particles = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        }

        float currentSpeed = 0.1f;
        float fadeTimer = 0f;
        Color originalColor = spriteRenderer.color;
        Vector3 originalPosition = transform.position;

        // Анимация подъёма и исчезновения
        while (fadeTimer < 4f)
        {
            // Поднимаем вверх с ускорением
            currentSpeed += 0.1f * Time.deltaTime;
            transform.position += Vector3.up * currentSpeed * Time.deltaTime;

            // Плавное исчезновение
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / 4f);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        // Спавним предмет на оригинальной позиции монстра
        if (itemDropPrefab != null)
        {
            Instantiate(itemDropPrefab, originalPosition, Quaternion.identity);
        }

        // Ждём немного перед уничтожением
        yield return new WaitForSeconds(1f);

        // Уничтожаем монстра
        Destroy(gameObject);
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
}