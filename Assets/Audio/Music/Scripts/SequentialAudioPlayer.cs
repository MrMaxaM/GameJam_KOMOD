using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SequentialAudioPlayer : MonoBehaviour
{
    private AudioSource source;
    private AudioClip[] clips;
    private int currentIndex;
    private bool isActive;

    private AudioClip[] pendingClips;
    private double nextStartTime;   // время начала следующего клипа

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    void Update()
    {
        if (!isActive || clips == null || clips.Length == 0)
            return;

        // Планируем следующий клип чуть заранее, пока предыдущий почти доигрывает
        if (AudioSettings.dspTime + 0.1f >= nextStartTime)
        {
            ScheduleNextClip();
        }
    }

    public void PlayPlaylist(AudioClip[] newClips, bool force)
    {
        if (newClips == null || newClips.Length == 0) return;

        if (isActive && !force)
        {
            pendingClips = newClips; // переключим после текущего
        }
        else
        {
            clips = newClips;
            currentIndex = 0;
            isActive = true;
            nextStartTime = 0;
            ScheduleNextClip();
        }
    }

    public void StopPlaylist()
    {
        isActive = false;
        pendingClips = null;
        source.Stop();
    }

    private void ScheduleNextClip()
    {
        if (clips == null || clips.Length == 0) return;

        if (pendingClips != null)
        {
            clips = pendingClips;
            pendingClips = null;
            currentIndex = 0;
        }

        AudioClip clip = clips[currentIndex];
        if (clip == null) return;

        double startTime = nextStartTime > 0 ? nextStartTime : AudioSettings.dspTime;
        source.clip = clip;
        source.PlayScheduled(startTime);

        nextStartTime = startTime + clip.length;
        currentIndex++;
        if (currentIndex >= clips.Length)
            currentIndex = 0;
    }
}
