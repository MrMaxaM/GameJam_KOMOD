using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;
    public bool canMove = true;

    private Vector2 smoothVelocity;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    void FixedUpdate()
    {
        if (!canMove) 
        {
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, Vector2.zero, 
                                                   ref smoothVelocity, smoothTime);
            return;
        }

            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, move * moveSpeed, 
                                                   ref smoothVelocity, smoothTime);
    }
}
