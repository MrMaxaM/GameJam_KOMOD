using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraFollow : MonoBehaviour
{
    [Header("Track Settings")]
    public Transform target;
    public float smoothTime = 0.3f;
    public float smoothOrthoTime = 2f;
    
    [Header("Post Processing")]
    public Volume postProcessVolume;
    
    [Header("Threat Effects")]
    public float maxThreatDistance = 5f;
    public float maxChromaticAberration = 0.3f;
    public float maxVignette = 0.4f;
    public float maxFilmGrain = 0.3f;
    public float sizeMultiplier = 1.2f;
    
    private ChromaticAberration chromatic;
    private Vignette vignette;
    private FilmGrain grain;
    private Vector3 originalPosition;

    private Vector3 velocity = Vector3.zero;
    private Camera maincamera;
    private float defaultSize;
    private float currentSize;
    private float targetSize;
    private float defaultVignette;
    private float defaultGrain;

    void Start()
    {
        Fade.Instance.FadeFromBlack(2f);
        originalPosition = transform.localPosition;
        maincamera = GetComponent<Camera>();
        defaultSize = maincamera.orthographicSize;
        currentSize = defaultSize;

        // Получаем компоненты пост-процессинга
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out chromatic))
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out grain);
            defaultVignette = vignette.intensity.value;
            defaultGrain = grain.intensity.value;
        }
        
        ResetEffects();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        targetPosition.z = -10f;
        targetPosition.y += 0.4f;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        currentSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * smoothOrthoTime);
        Camera.main.orthographicSize = currentSize;
    }

    // Вызывается монстрами для обновления эффектов угрозы
    public void UpdateThreatEffect(float distance, bool isChasing)
    {
        if (!isChasing)
        {
            ResetEffects();
            return;
        }

        float threatLevel = 1f - Mathf.Clamp01(distance / maxThreatDistance);

        chromatic.intensity.value = threatLevel * maxChromaticAberration;
        vignette.intensity.value = defaultVignette + (threatLevel * maxVignette);
        grain.intensity.value = defaultGrain + (threatLevel * maxFilmGrain);
        targetSize = defaultSize + defaultSize * (sizeMultiplier * threatLevel);
    }
    
    void ResetEffects()
    {
        if (chromatic != null) chromatic.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = defaultVignette;
        if (grain != null) grain.intensity.value = defaultGrain;
        targetSize = defaultSize;
    }
}