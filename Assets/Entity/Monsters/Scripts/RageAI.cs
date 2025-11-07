using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;


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
    public GameObject deathParticlesPrefab;
    public GameObject itemDropPrefab; 
    
    [Header("Destruction")]
    public GameObject destructionEffect;
    
    private NavMeshAgent agent;
    private Transform player;
    
    private HideController hideController;
    private PlayerController playerController;
    private Vector3 lastHeardPosition;
    private float stateTimer;
    private float attackTimer;
    
    private enum AIState { Wandering, Chasing, PreparingCharge, Charging, Searching, Dying }
    private AIState currentState = AIState.Wandering;
    
    private Vector3 chargeDirection;
    private float chargeTimer;
    private SpriteRenderer spriteRenderer;
    private Collider2D monsterCollider;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 move;

    private PlaylistManager playlistManager;

    public AudioClip attackClip;           // звук атаки
    public AudioClip[] chaseClips;         // набор звуков для преследования

    private AudioSource audioSource;
    private Coroutine chaseSoundRoutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        hideController = GameObject.FindGameObjectWithTag("Player").GetComponent<HideController>();
        currentHearingRange = normalHearingRange;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        rb = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        playlistManager = GetComponent<PlaylistManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetWanderDestination();
    }

    void Update()
    {
        UpdateTimers();
        UpdateHearingRange();

        move = agent.velocity;

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
                move = rb.linearVelocity;
                break;
            case AIState.Searching:
                UpdateSearching();
                break;
        }
        
        // Обновляем последнее направление если есть движение
        if (move.magnitude > 0.1f)
        {
            animator.SetFloat("RLastX", move.x);
        }
        
        // Устанавливаем параметры движения
        animator.SetFloat("RX", move.x);
        
        // Устанавливаем булевые параметры
        animator.SetBool("RisWalking", move.magnitude > 0.1f || currentState == AIState.Charging);
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
        return distanceToPlayer <= currentHearingRange & !hideController.isHiding;
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

        PlaySound(attackClip);
        
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

        if (DialogueState.Instance.currentPlace == "present")
            playlistManager.PlayPlaylist("RageCalm");
        StopChaseSounds();
    }

    void StartChasing()
    {
        currentState = AIState.Chasing;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lastHeardPosition = player.position;

        if (DialogueState.Instance.currentPlace == "present")
            playlistManager.PlayPlaylist("RageChasing", true);
        StartChaseSounds();

    }

    void StartSearching()
    {
        currentState = AIState.Searching;
        agent.SetDestination(lastHeardPosition);
        stateTimer = waitTimeAtPoint;
        if (DialogueState.Instance.currentPlace == "present")
            playlistManager.PlayPlaylist("RageSearching");

        StopChaseSounds();
    }
    public void StartDying()
    {
        Debug.Log($"СМЭРТЬ!");
        currentState = AIState.Dying;
        agent.isStopped = true;
        StartCoroutine(DeathAnimation());
    }
    
    public void UpdateLocation(string newLocation)
    {
        if (playlistManager != null && newLocation == "past")
            playlistManager.Stop();
        else
            playlistManager.PlayPlaylist("RageCalm", true);
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

    //

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void StartChaseSounds()
    {
        Debug.Log($"Старт звуков преследования");
        StopChaseSounds();
        if (chaseClips != null && chaseClips.Length > 0)
            chaseSoundRoutine = StartCoroutine(PlayRandomChaseSounds());
    }

    void StopChaseSounds()
    {
        if (chaseSoundRoutine != null)
        {
            StopCoroutine(chaseSoundRoutine);
            chaseSoundRoutine = null;
        }
    }

    IEnumerator PlayRandomChaseSounds()
    {
        while (currentState == AIState.Chasing)
        {
            float wait = Random.Range(1f, 2.5f);               // интервал между звуками
            yield return new WaitForSeconds(wait);

            var clip = chaseClips[Random.Range(0, chaseClips.Length)];
            PlaySound(clip);
            Debug.Log($"Кричу: {clip}");
        }
    }
}

