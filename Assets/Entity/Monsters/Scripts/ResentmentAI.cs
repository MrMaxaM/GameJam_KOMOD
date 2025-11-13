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
    public GameObject spawnParticlesPrefab;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;

    [Header("Debug Info")]
    [SerializeField] private float attackTimer;
    [SerializeField] private AIState currentState = AIState.Idle;
    [SerializeField] private float stateTimer;
    [SerializeField] private Vector3 lastKnownPlayerPosition;

    [Header("Audio Settings")]
    public AudioClip spawnClip;
    public AudioClip chaseClip;
    public AudioClip attackClip;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Transform player;
    
    private HideController hideController;
    private PuddleController homePuddle;
    private Vector3 puddlePosition;
    private enum AIState { Idle, Chasing, Attacking, Returning, Searching }
    private Animator animator;
    private Vector2 move;
    private CameraFollow cameraEffects;
    private float distanceToPlayer;

    void Start()
    {
        cameraEffects = Camera.main.GetComponent<CameraFollow>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = chaseSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hideController = GameObject.FindGameObjectWithTag("Player").GetComponent<HideController>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        Instantiate(spawnParticlesPrefab, transform.position, Quaternion.identity);
        PlaySound(spawnClip);
    }

    void Update()
    {
        UpdateTimers();

        move = agent.velocity;

        switch (currentState)
        {
            case AIState.Idle:
                Invoke(nameof(StartChasing),0.4f);
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
    }
    
    void UpdateChasing()
    {
        if (!CanSeePlayer())
        {
            StartSearching();
            return;
        }

        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);

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
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
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

        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }

        if (stateTimer > 0) return;

        StartReturning();
        cameraEffects.UpdateThreatEffect(distanceToPlayer, true);
    }
    
    void UpdateReturning()
    {
        float distanceToPuddle = Vector3.Distance(transform.position, puddlePosition);
        cameraEffects.UpdateThreatEffect(distanceToPlayer, false);
        
        if (distanceToPuddle <= 1.5f)
        {
            DisappearIntoPuddle();
            return;
        }
        
        if (CanSeePlayer())
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= returnToPuddleDistance * 0.7f)
            {
                StartChasing();
            }
        }
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
    
    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.isStopped = false;
        PlaySound(chaseClip);
    }

    void StartAttacking()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
        PlaySound(attackClip);
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
        cameraEffects.UpdateThreatEffect(distanceToPlayer, false);
    }
    
    void PerformAttack()
    {
        Debug.Log("Обида атаковала игрока!");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
        }

        Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
        Rigidbody2D playerRb = player.gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
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
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(clip);
    }
}