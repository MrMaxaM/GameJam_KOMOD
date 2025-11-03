using UnityEngine;

public class Closet : MonoBehaviour
{
    public SpriteRenderer closetSprite;
    public float lightnessBoost = 1.1f;

    private Color originalColor;
    private bool isOccupied = false;
    private HideController hideController;

    void Start()
    {
        if (closetSprite != null)
        {
            originalColor = closetSprite.color;
        }
    }

    Color AdjustBrightnessHSL(Color color, float lightBoost)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        
        v = Mathf.Min(v + lightBoost, 1.0f);
        
        return Color.HSVToRGB(h, s, v);
    }

    public void OnPlayerEnterRange()
    {
        if (!isOccupied && closetSprite != null)
        {
            closetSprite.color = originalColor * lightnessBoost;
        }
    }

    public void OnPlayerExitRange()
    {
        if (closetSprite != null && !isOccupied)
        {
            closetSprite.color = originalColor;
            hideController = null;
        }
    }

    public void OnPlayerHide(HideController hc)
    {
        isOccupied = true;

        hideController = hc;
        
        if (closetSprite != null)
        {
            closetSprite.color = originalColor;
        }
    }

    public void OnPlayerUnhide()
    {
        isOccupied = false;

        if (closetSprite != null)
        {
            closetSprite.color = originalColor * lightnessBoost;
        }
    }
    
    public void ForceExit()
    {
        if (hideController != null)
        {
            hideController.ForceExitCloset();
        }
    }
}