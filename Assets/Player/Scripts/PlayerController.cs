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

    private Vector2 smoothVelocity;
    private Rigidbody2D rb;
    private InputAction crouchAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        crouchAction = InputSystem.actions.FindAction("Crouch");
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

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, move * currentSpeed,
                                                ref smoothVelocity, smoothTime);
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
