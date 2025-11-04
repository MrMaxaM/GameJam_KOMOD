using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SequentialAudioPlayer : MonoBehaviour
{
    private AudioSource source;
    private AudioClip[] clips;
    private int currentIndex;
    private bool isActive;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isActive && clips != null && clips.Length > 0 && !source.isPlaying)
            PlayNextClip();
    }

    public void PlayPlaylist(AudioClip[] newClips)
    {
        if (newClips == null || newClips.Length == 0) return;

        clips = newClips;
        currentIndex = 0;
        isActive = true;
        PlayNextClip();
    }

    public void StopPlaylist()
    {
        isActive = false;
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
