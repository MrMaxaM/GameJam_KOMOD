using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class FakeResentment : MonoBehaviour
{
    public GameObject deathParticlesPrefab;
    public GameObject circleParticlePrefab;
    public GameObject circlePrefab;
    public GameObject itemSpawnPerticlesPrefab;
    public GameObject itemDropPrefab;
    public GameObject monologPrefab;
    private Material materialRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        materialRenderer = GetComponent<Renderer>().material;
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
            Instantiate(circleParticlePrefab, transform.position, Quaternion.Euler(-90, 0, 0));
            Instantiate(circlePrefab, transform.position, Quaternion.identity);
            GameObject newObject = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity, transform);
            //newObject.transform.SetParent(transform);
        }

        float currentSpeed = 0.1f;
        float fadeTimer = 0f;
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
            float alpha = 1.3f - Mathf.Lerp(0, 1.3f, fadeTimer / 4f);
            materialRenderer.SetFloat("_DissolveAmount", alpha);

            yield return null;
        }

        // Спавним предмет на оригинальной позиции монстра
        if (itemDropPrefab != null)
        {
            Instantiate(itemDropPrefab, originalPosition + Vector3.up * 0.5f, Quaternion.identity);
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