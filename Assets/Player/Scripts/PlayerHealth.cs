using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHearts = 3;
    public int currentHearts;
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Audio Settings")]
    public AudioClip damageClip;     // звук при получении урона
    public AudioClip deathClip;      // звук смерти
    [Range(0f, 1f)] public float volume = 0.8f;

    public UnityEvent OnHeartsChanged;
    public UnityEvent OnPlayerDeath;

    private PlayerController playerController;
    private AudioSource audioSource;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        currentHearts = maxHearts;
        OnHeartsChanged?.Invoke();
    }
    
    public void TakeDamage()
    {
        if (currentHearts <= 0) return;
        currentHearts -= 1;
        OnHeartsChanged?.Invoke();
        Debug.Log($"Игрок получил урон! Осталось сердец: {currentHearts}");
        PlaySound(damageClip);

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Fade.Instance.FadeInOut(1f);
        playerController.canMove = false;
        GetComponent<InventorySystem>().DropAllItems();
        OnPlayerDeath?.Invoke();
        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            Debug.LogError($"Нет точки респавна!");
        }
        
        currentHearts = maxHearts;
        playerController.canMove = true;
        OnHeartsChanged?.Invoke();
        
        Debug.Log("Игрок возродился!");
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
