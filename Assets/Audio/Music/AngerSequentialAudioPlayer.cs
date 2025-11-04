using UnityEngine;

public class AngerSequentialAudioPlayer : MonoBehaviour
{
    public AudioClip[] clips;      // Массив файлов .wav
    private AudioSource source;
    private int currentIndex = 0;

    void Start()
    {
        source = GetComponent<AudioSource>();
        if (clips.Length > 0)
        {
            PlayNextClip();
        }
    }

    void Update()
    {
        // Проверка, закончился ли текущий трек
        if (!source.isPlaying && clips.Length > 0)
        {
            PlayNextClip();
        }
    }

    void PlayNextClip()
    {
        source.clip = clips[currentIndex];
        source.Play();

        currentIndex++;
        if (currentIndex >= clips.Length)
        {
            currentIndex = 0; // или удалить строку, если не нужно зацикливать
        }
    }
}
