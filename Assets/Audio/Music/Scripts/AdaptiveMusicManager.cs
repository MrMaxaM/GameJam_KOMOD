using UnityEngine;
using System.Collections;

public class AdaptiveMusicManager : MonoBehaviour
{
    public AudioSource calmMusic;
    public AudioSource chaseMusic;
    public AudioSource searchMusic;

    [Range(0f, 1f)] public float targetVolume = 0.7f;
    public float fadeSpeed = 1f; // скорость перехода между состояниями

    private AudioSource currentSource;

    public enum MonsterState { Calm, Chase, Search }
    private MonsterState currentState = MonsterState.Calm;

    void Start()
    {
        calmMusic.loop = true;
        chaseMusic.loop = true;
        searchMusic.loop = true;

        calmMusic.Play();
        chaseMusic.Play();
        searchMusic.Play();

        calmMusic.volume = targetVolume;
        chaseMusic.volume = 0f;
        searchMusic.volume = 0f;

        currentSource = calmMusic;
    }

    void Update()
    {
        // Плавное выравнивание громкости каждый кадр
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        float step = fadeSpeed * Time.deltaTime;

        float calmTarget = currentState == MonsterState.Calm ? targetVolume : 0f;
        float chaseTarget = currentState == MonsterState.Chase ? targetVolume : 0f;
        float searchTarget = currentState == MonsterState.Search ? targetVolume : 0f;

        calmMusic.volume = Mathf.MoveTowards(calmMusic.volume, calmTarget, step);
        chaseMusic.volume = Mathf.MoveTowards(chaseMusic.volume, chaseTarget, step);
        searchMusic.volume = Mathf.MoveTowards(searchMusic.volume, searchTarget, step);
    }

    public void SetState(MonsterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    // Пример вызова: из кода монстра
    // musicManager.SetState(AdaptiveMusicManager.MonsterState.Chase);
}
