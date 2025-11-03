using UnityEngine;

public class HideController : MonoBehaviour
{
    [Header("Hiding Settings")]
    public float interactionRadius = 1.5f;
    public KeyCode interactionKey = KeyCode.E;
    public bool isHiding = false;
    public SpriteRenderer playerSprite;

    private Closet currentCloset;
    private Vector3 exitPosition;
    private GameObject activeHiddenEffect;

    void Update()
    {
        CheckNearbyClosets();
        HandleHidingInput();
    }

    void CheckNearbyClosets()
    {
        // Ищем шкафы в радиусе взаимодействия
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        
        Closet closestCloset = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in nearbyColliders)
        {
            Closet closet = collider.GetComponent<Closet>();
            if (closet != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCloset = closet;
                }
            }
        }
        
        // Обновляем текущий шкаф
        if (currentCloset != closestCloset)
        {
            if (currentCloset != null)
            {
                currentCloset.OnPlayerExitRange();
                if (closestCloset == null && isHiding)
                {
                    ForceExitCloset();
                } 
            }
            
            currentCloset = closestCloset;

            if (currentCloset != null && !isHiding)
            {
                currentCloset.OnPlayerEnterRange();
            }
            
            
        }
    }

    void HandleHidingInput()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            if (!isHiding && currentCloset != null)
            {
                EnterCloset();
            }
            else if (isHiding)
            {
                ExitCloset();
            }
        }
    }

    void EnterCloset()
    {
        if (currentCloset == null || isHiding) return;
        
        exitPosition = transform.position;
        
        transform.position = currentCloset.transform.position;
        
        SetHidingState(true);
        
        currentCloset.OnPlayerHide(this);
        
        Debug.Log("Спрятались в шкафу!");
    }

    void ExitCloset()
    {
        if (!isHiding) return;
        transform.position = exitPosition;
        
        SetHidingState(false);
        
        if (currentCloset != null)
        {
            currentCloset.OnPlayerUnhide();
        }
        
        Debug.Log("Вышли из шкафа!");
    }

    void SetHidingState(bool hiding)
    {
        isHiding = hiding;

        if (playerSprite != null)
        {
            playerSprite.enabled = !hiding;
        }
        
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = !hiding;
        }
        
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.canMove = !hiding;
        }
    }

    public bool IsHiding() => isHiding;
    
    public void ForceExitCloset()
    {
        if (isHiding)
        {
            ExitCloset();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}