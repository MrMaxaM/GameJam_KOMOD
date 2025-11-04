using UnityEngine;
using System.Collections;

public class PuddleController : MonoBehaviour
{
    [Header("Puddle Settings")]
    public float maxSize = 3f;
    public float growthSpeed = 0.5f;
    public float lifeDuration = 12f;
    public float spawnMonsterRange = 2f;
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("References")]
    public GameObject resentmentPrefab;
    public CapsuleCollider2D slowArea;
    
    private float currentSize;
    private float lifeTimer;
    private bool isActive = true;
    private ResentmentAI spawnedMonster;
    private Transform player;
    private PuddleSpawner spawner;
    private SpriteRenderer spriteRenderer;

    public void Initialize(PuddleSpawner puddleSpawner)
    {
        spawner = puddleSpawner;
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentSize = 0.1f;
        lifeTimer = lifeDuration;
        transform.localScale = Vector3.one * currentSize;

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        StartCoroutine(GrowPuddle());
    }
    
    void Update()
    {
        if (!isActive) return;
        
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Disappear();
        }
    }
    
    IEnumerator GrowPuddle()
    {
        float startSize = 0.1f;
        float growthDuration = (maxSize - startSize) / growthSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < growthDuration && isActive)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / growthDuration);
            float curveValue = growthCurve.Evaluate(progress);
            
            currentSize = Mathf.Lerp(startSize, maxSize, curveValue);
            transform.localScale = Vector3.one * currentSize;
            
            yield return null;
        }
        
        currentSize = maxSize;
        transform.localScale = Vector3.one * currentSize;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            Invoke(nameof(TrySpawnMonster), 0.4f);
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ApplySlow();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Снятие замедления
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.RemoveSlow();
            }
        }
    }
    
    void TrySpawnMonster()
    {
        Debug.Log("Пробуем спавнить монстра...");
        if (spawnedMonster != null || !spawner.CanSpawnMonster()) return;
        
        GameObject monsterObj = Instantiate(resentmentPrefab, transform.position, Quaternion.identity);
        spawnedMonster = monsterObj.GetComponent<ResentmentAI>();
        spawnedMonster.SetHomePuddle(this);
        spawner.OnMonsterSpawned();
        Debug.Log("Монстр заспавнен!");
    }
    
    public void ReturnMonster()
    {
        if (spawnedMonster != null)
        {
            spawner.OnMonsterReturned();
            spawnedMonster = null;
        }
    }
    
    public void Disappear()
    {
        if (spawnedMonster != null)
        {
            lifeTimer += 1f;
            return;
        }

        spawner.OnPuddleDestroyed(this);
        isActive = false;
        StartCoroutine(DisappearAnimation());
    }
    
    IEnumerator DisappearAnimation()
    {
        float disappearTime = 20f;
        float timer = disappearTime;
        
        while (timer > 0)
        {
            float scale = Mathf.Lerp(maxSize, currentSize, timer / disappearTime);
            transform.localScale = Vector3.one * scale;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a -= 0.1f;
                spriteRenderer.color = color;
            }

            timer -= Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}