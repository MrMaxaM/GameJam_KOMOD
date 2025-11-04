using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 5f;
    public float slowedSpeed = 2f;
    public float smoothTime = 0.1f;
    public bool canMove = true;
    public bool isCrouching;
    private bool isSlowed = false;
    public bool flipSpriteForLeft = true;

    private Vector2 smoothVelocity;
    private Rigidbody2D rb;
    private InputAction crouchAction;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 move;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        crouchAction = InputSystem.actions.FindAction("Crouch");
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, Vector2.zero,
                                                   ref smoothVelocity, smoothTime);
            return;
        }

        isCrouching = crouchAction.IsPressed() || isSlowed;
        float currentSpeed = isCrouching ? slowedSpeed : normalSpeed;
        
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, move * currentSpeed,
                                                ref smoothVelocity, smoothTime);

        UpdateAnimationParameters(move);
    }

    void UpdateAnimationParameters(Vector2 movement)
    {
        if (animator == null) return;

        // Обновляем последнее направление если есть движение
        if (movement.magnitude > 0.1f)
        {
            animator.SetFloat("LastX", movement.x);
            animator.SetFloat("LastY", movement.y);
        }
        
        // Устанавливаем параметры движения
        animator.SetFloat("X", movement.x);
        animator.SetFloat("Y", movement.y);
        
        // Устанавливаем булевые параметры
        animator.SetBool("isWalking", movement.magnitude > 0.1f);
        animator.SetBool("isCrouching", isCrouching);

        if (spriteRenderer == null || !flipSpriteForLeft) return;

        // Отражаем спрайт только по горизонтали
        if (movement.x < -0.1f) // Движение влево
        {
            spriteRenderer.flipX = true;
        }
        else // Движение вправо
        {
            spriteRenderer.flipX = false;
        }
    }
    
    public void ApplySlow()
    {
        isSlowed = true;
    }
    
    public void RemoveSlow()
    {
        isSlowed = false;
    }
}
