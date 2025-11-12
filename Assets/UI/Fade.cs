using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Fade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    
    private Image fadeImage;
    private CanvasGroup canvasGroup;
    private Coroutine currentFade;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Находим или создаём компоненты
        fadeImage = GetComponent<Image>();
        if (fadeImage == null)
        {
            fadeImage = gameObject.AddComponent<Image>();
            fadeImage.color = Color.black;
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Настраиваем Canvas для поверх всех
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        // Изначально скрываем
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    // Вызов из любого места: FadeEffect.Instance.FadeToBlack();
    public static Fade Instance { get; private set; }

    // Из чёрного в прозрачный (появление)
    public void FadeFromBlack()
    {
        StartFade(canvasGroup.alpha, 0f);
    }

    // Из прозрачного в чёрный (исчезновение)
    public void FadeToBlack()
    {
        StartFade(canvasGroup.alpha, 1f);
    }

    // Полный переход: прозрачное → чёрное → прозрачное
    public void FadeInOut()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeInOutRoutine());
    }

    private void StartFade(float from, float to)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        // Блокируем клики когда не полностью прозрачен
        canvasGroup.blocksRaycasts = (to > 0f);
        canvasGroup.interactable = (to > 0f);

        float timer = 0f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = to;
        
        // Разблокируем клики если полностью прозрачен
        if (to == 0f)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        currentFade = null;
    }

    private IEnumerator FadeInOutRoutine()
    {
        // Блокируем клики на время всего перехода
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // Прозрачное → Чёрное
        yield return FadeRoutine(0f, 1f);
        
        // Задержка между фазами
        yield return new WaitForSeconds(0.2f);
        
        // Чёрное → Прозрачное
        yield return FadeRoutine(1f, 0f);
        
        // Разблокируем клики
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        currentFade = null;
    }

    // Публичные методы для вызова из других скриптов
    public void FadeFromBlack(float duration)
    {
        fadeDuration = duration;
        FadeFromBlack();
    }

    public void FadeToBlack(float duration)
    {
        fadeDuration = duration;
        FadeToBlack();
    }

    public void FadeInOut(float duration)
    {
        fadeDuration = duration;
        FadeInOut();
    }
}