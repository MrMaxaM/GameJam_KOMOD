using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip backgroundMusic;
    public float fadeInTime = 2f;
    public float fadeOutTime = 2f;
    public bool playOnStart = true;
    public bool loop = true;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
        }

        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("No background music assigned!");
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInMusic());
    }

    public void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeInMusic()
    {
        audioSource.clip = backgroundMusic;
        audioSource.Play();

        float startVolume = 0f;
        audioSource.volume = startVolume;

        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 1f, timer / fadeInTime);
            yield return null;
        }

        audioSource.volume = 1f;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = audioSource.volume;

        float timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    // Для смены музыки
    public void ChangeMusic(AudioClip newMusic)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        StartCoroutine(CrossFadeMusic(newMusic));
    }

    private IEnumerator CrossFadeMusic(AudioClip newMusic)
    {
        // Фейд-аут текущей музыки
        yield return StartCoroutine(FadeOutMusic());

        // Меняем клип
        backgroundMusic = newMusic;

        // Фейд-ин новой музыки
        yield return StartCoroutine(FadeInMusic());
    }
    
    // Автоматически останавливаем музыку при уничтожении объекта
    void OnDestroy()
    {
        StopMusic();
    }
}