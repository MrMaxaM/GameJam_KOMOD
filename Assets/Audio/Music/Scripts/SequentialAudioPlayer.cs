using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SequentialAudioPlayer : MonoBehaviour
{
    private AudioSource source;
    private AudioClip[] clips;
    private int currentIndex;
    private bool isActive;

    private AudioClip[] pendingClips;   // новый плейлист в ожидании

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isActive && clips != null && clips.Length > 0 && !source.isPlaying)
        {
            // если есть отложенная замена — применяем
            if (pendingClips != null)
            {
                clips = pendingClips;
                pendingClips = null;
                currentIndex = 0;
            }

            PlayNextClip();
        }
    }

    public void PlayPlaylist(AudioClip[] newClips)
    {
        if (newClips == null || newClips.Length == 0) return;

        // если уже что-то играет — ждем конца
        if (isActive && source.isPlaying)
        {
            pendingClips = newClips;
        }
        else
        {
            clips = newClips;
            currentIndex = 0;
            isActive = true;
            PlayNextClip();
        }
    }

    public void StopPlaylist()
    {
        isActive = false;
        pendingClips = null;
        source.Stop();
    }

    private void PlayNextClip()
    {
        if (clips == null || clips.Length == 0) return;

        source.clip = clips[currentIndex];
        source.Play();

        currentIndex++;
        if (currentIndex >= clips.Length)
            currentIndex = 0;
    }
}