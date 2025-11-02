using UnityEngine;
using UnityEngine.UI;

public class UIHearts : MonoBehaviour
{
    public Image[] heartImages;
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerHealth.OnHeartsChanged.AddListener(UpdateHeartsUI);
        UpdateHeartsUI();
    }

    void UpdateHeartsUI()
    {
        if (playerHealth == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = i < playerHealth.currentHearts;
        }
    }
}
