using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

public class PulseEffect : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float bloomMinIntensity = 0.5f;
    public float bloomMaxIntensity = 2f;
    public float bloomPulseSpeed = 2f;
    public float lightMinIntensity = 0.5f;
    public float lightMaxIntensity = 2f;
    public float lightPulseSpeed = 2f;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;

    private Bloom bloom;
    private Light2D targetLight;
    private SpriteRenderer circleRenderer;
    private float bloomPulseTime;
    private float lightPulseTime;
    private bool isFading = false;

    [System.Obsolete]
    void Start()
    {
        // Автоматически находим Global Volume
        Volume volume = FindObjectOfType<Volume>();
        if (volume != null)
        {
            volume.profile.TryGet(out bloom);
        }

        // Автоматически находим Light2D на этом объекте
        targetLight = GetComponent<Light2D>();
        
        // Если нет Light2D на этом объекте, ищем любой в сцене
        if (targetLight == null)
        {
            targetLight = FindObjectOfType<Light2D>();
        }

        // Находим SpriteRenderer для круга
        circleRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFading) return;

        // Пульсация Bloom
        if (bloom != null)
        {
            bloomPulseTime += Time.deltaTime * bloomPulseSpeed;
            float bloomIntensity = Mathf.Lerp(bloomMinIntensity, bloomMaxIntensity, 
                (Mathf.Sin(bloomPulseTime) + 1f) * 0.5f);
            bloom.intensity.value = bloomIntensity;
        }

        // Пульсация Light2D
        if (targetLight != null)
        {
            lightPulseTime += Time.deltaTime * lightPulseSpeed;
            float lightIntensity = Mathf.Lerp(lightMinIntensity, lightMaxIntensity, 
                (Mathf.Sin(lightPulseTime) + 1f) * 0.5f);
            targetLight.intensity = lightIntensity;
        }
    }

    // Запуск исчезновения
    public void StartFadeOut()
    {
        if (!isFading)
        {
            isFading = true;
            StartCoroutine(FadeOutRoutine());
        }
    }

    System.Collections.IEnumerator FadeOutRoutine()
    {
        float timer = 0f;
        Color originalColor = circleRenderer.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Плавное уменьшение прозрачности
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(originalColor.a, 0f, progress);
            circleRenderer.color = newColor;

            // Также уменьшаем интенсивность света
            if (targetLight != null)
            {
                targetLight.intensity = Mathf.Lerp(lightMaxIntensity, 0f, progress);
            }

            yield return null;
        }

        // Удаляем объект после исчезновения
        Destroy(gameObject);
    }

    // Методы для ручного управления
    public void SetBloomPulseSpeed(float speed) => bloomPulseSpeed = speed;
    public void SetLightPulseSpeed(float speed) => lightPulseSpeed = speed;
}