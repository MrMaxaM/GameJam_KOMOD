using UnityEngine;
using System.Collections;

public class FakeResentment : MonoBehaviour
{
    public GameObject deathParticlesPrefab;
    public GameObject itemSpawnPerticlesPrefab;
    public GameObject itemDropPrefab;
    public GameObject monologPrefab;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void StartDying()
    {
        Debug.Log($"СМЭРТЬ!");
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        
        yield return new WaitForSeconds(0.4f);

        // Спавним партиклы смерти
        if (deathParticlesPrefab != null)
        {
            GameObject particles = Instantiate(deathParticlesPrefab, transform.position, Quaternion.Euler(-90, 0, 0));
        }

        float currentSpeed = 0.1f;
        float fadeTimer = 0f;
        Color originalColor = spriteRenderer.color;
        Vector3 originalPosition = transform.position;
        originalPosition.z = 0f;

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
            Instantiate(itemSpawnPerticlesPrefab, originalPosition, Quaternion.Euler(-90, 0, 0));
            if (monologPrefab != null)
                Instantiate(monologPrefab, originalPosition, Quaternion.identity);
        }

        // Ждём немного перед уничтожением
        yield return new WaitForSeconds(1f);

        // Уничтожаем монстра
        Destroy(gameObject);
    }
}