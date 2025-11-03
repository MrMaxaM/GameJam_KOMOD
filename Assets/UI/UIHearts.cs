using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHearts : MonoBehaviour
{
    [Header("Heart Settings")]
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite damagedHeartSprite;
    public float blinkDuration = 0.2f;
    public int blinkCount = 2;
    
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHeartsChanged.AddListener(UpdateHeartsUI);
        }
    }

    IEnumerator AnimateHeartLoss(int heartIndex)
    {
        Image heartImage = heartImages[heartIndex];
        
        // Мигание 2 раза
        for (int i = 0; i < blinkCount; i++)
        {
            // Меняем на спрайт урона
            heartImage.sprite = damagedHeartSprite;
            yield return new WaitForSeconds(blinkDuration);
            
            // Возвращаем обычный спрайт
            heartImage.sprite = fullHeartSprite;
            yield return new WaitForSeconds(blinkDuration);
        }
        
        // После мигания - скрываем сердце
        heartImage.enabled = false;
    }

    void UpdateHeartsUI()
    {
        if (playerHealth == null) return;

        int lostHeartIndex = playerHealth.currentHearts;
        if (lostHeartIndex >= 0 && lostHeartIndex < heartImages.Length)
        {
            StartCoroutine(AnimateHeartLoss(lostHeartIndex));
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < playerHealth.currentHearts)
            {
                // Активное сердце
                heartImages[i].enabled = true;
                heartImages[i].sprite = fullHeartSprite;
            }
        }
    }
}