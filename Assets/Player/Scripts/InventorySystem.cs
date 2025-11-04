using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 3;
    public float pickupRadius = 2f;
    public KeyCode pickupKey = KeyCode.E;

    public KeyCode[] dropKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    public KeyCode dropAllKey = KeyCode.G;
    
    [Header("UI References")]
    public Image[] slotImages; // 3 Image для слотов UI
    public Sprite emptySlotSprite;
    
    [Header("Item Settings")]
    public LayerMask itemLayers = 1; // Слои для предметов
    
    public List<Item> inventory = new List<Item>();
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        InitializeInventory();
    }

    void Update()
    {
        HandleInput();
        UpdateUI();
    }

    void InitializeInventory()
    {
        // Заполняем инвентарь пустыми слотами
        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(null);
        }
    }

    void HandleInput()
    {
        // Подбор предмета
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupItem();
        }
        
        // Выбрасывание предметов по цифрам
        for (int i = 0; i < dropKeys.Length && i < inventorySize; i++)
        {
            if (Input.GetKeyDown(dropKeys[i]))
            {
                DropItem(i);
            }
        }
        
        // Выбрасывание всех предметов
        if (Input.GetKeyDown(dropAllKey))
        {
            DropAllItems();
        }
    }

    void TryPickupItem()
    {
        // Ищем ближайший предмет в радиусе
        Collider2D[] nearbyItems = Physics2D.OverlapCircleAll(transform.position, pickupRadius, itemLayers);
        
        Item closestItem = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider2D collider in nearbyItems)
        {
            Item item = collider.GetComponent<Item>();
            if (item != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }
        
        // Подбираем ближайший предмет
        if (closestItem != null)
        {
            PickupItem(closestItem);
        }
    }

    void PickupItem(Item item)
    {
        // Ищем свободный слот
        int freeSlot = -1;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                freeSlot = i;
                break;
            }
        }
        
        if (freeSlot != -1)
        {
            // Добавляем в инвентарь
            inventory[freeSlot] = item;
            item.OnPickup(this);

            Debug.Log($"Подобран предмет: {item.itemName} в слот {freeSlot + 1}");
            return;
        }
        else
        {
            Debug.Log("Инвентарь полон!");
        }
    }

    public void DropItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Count) return;

        Item item = inventory[slotIndex];
        if (item != null)
        {
            // Выбрасываем предмет
            Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle * 0.3f;
            item.OnDrop(dropPosition);
            inventory[slotIndex] = null;

            Debug.Log($"Выброшен предмет: {item.itemName} из слота {slotIndex + 1}");
        }
    }
    
        public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Count) return;
        
        Item item = inventory[slotIndex];
        if (item != null)
        {
            // Удаляем предмет
            inventory[slotIndex] = null;
            
            Debug.Log($"Удалён предмет: {item.itemName} из слота {slotIndex + 1}");
        }
    }

    public void DropAllItems()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                DropItem(i);
            }
        }
        
        Debug.Log("Все предметы выброшены!");
    }

    void UpdateUI()
    {
        // Обновляем UI инвентаря
        for (int i = 0; i < slotImages.Length && i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                slotImages[i].sprite = inventory[i].icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = emptySlotSprite;
                slotImages[i].color = new Color(1, 1, 1, 0.3f); // Полупрозрачный
            }
        }
    }

    // Публичные методы для доступа из других скриптов
    public int HasItem(Item item)
    {
        if (inventory.Contains(item))
        {
            return inventory.IndexOf(item);
        }

        return -1;
    }
    
    public int HasItemByName(string name)
    {
        foreach (Item item in inventory)
        {
            if (item != null && item.itemName == name)
                return inventory.IndexOf(item);
        }
        return -1;
    }
    
    public int GetItemCount()
    {
        int count = 0;
        foreach (Item item in inventory)
        {
            if (item != null) count++;
        }
        return count;
    }
    
    public bool IsFull()
    {
        return GetItemCount() >= inventorySize;
    }

    // Визуализация радиуса подбора в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}