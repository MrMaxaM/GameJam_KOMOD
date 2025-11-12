using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName;
    public Sprite icon;
    
    protected InventorySystem inventory;
    protected Collider2D itemCollider;
    protected SpriteRenderer spriteRenderer;

    void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (icon == null)
        {
            icon = spriteRenderer.sprite;
        }
    }

    public void OnPickup(InventorySystem collector)
    {
        inventory = collector;
        
        // Отключаем видимость и коллайдер
        itemCollider.enabled = false;
        spriteRenderer.enabled = false;
        
        // Делаем дочерним объектом (опционально)
        transform.SetParent(collector.transform);
        transform.localPosition = Vector3.zero;

        if(itemName == "Кнопка")
        {
            // PulseEffect circle = FindFirstObjectByType<PulseEffect>();
            // circle.StartFadeOut();
        }
    }

    public void OnDrop(Vector2 dropPosition)
    {
        // Включаем обратно
        itemCollider.enabled = true;
        spriteRenderer.enabled = true;
        
        // Возвращаем в мир
        transform.SetParent(null);
        transform.position = dropPosition;
        
        inventory = null;
    }
}