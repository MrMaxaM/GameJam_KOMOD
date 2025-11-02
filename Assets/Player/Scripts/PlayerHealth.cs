using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    public int currentHearts;
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    public UnityEvent OnHeartsChanged;
    public UnityEvent OnPlayerDeath;

    private PlayerController playerController;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        currentHearts = maxHearts;
        OnHeartsChanged?.Invoke();
    }
    
    public void TakeDamage()
    {
        if (currentHearts <= 0) return;
        currentHearts -= 1;
        OnHeartsChanged?.Invoke();
        Debug.Log($"Игрок получил урон! Осталось сердец: {currentHearts}");
        
        if (currentHearts <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerController.canMove = false;
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
}
