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

    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;   // набор шагов
    public float stepInterval = 0.4f;   // задержка между шагами
    public float volume = 0.6f;

    private float stepTimer;
    private AudioSource audioSource;
    private Vector2 smoothVelocity;
    private Rigidbody2D rb;
    private InputAction crouchAction;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
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

        HandleFootsteps(move);
    }

    void HandleFootsteps(Vector2 move)
    {
        if (move.magnitude > 0.1f && rb.linearVelocity.magnitude > 0.1f)
        {
            stepTimer -= Time.fixedDeltaTime;
            if (stepTimer <= 0f && footstepClips.Length > 0)
            {
                AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
                audioSource.PlayOneShot(clip, volume);
                stepTimer = stepInterval / (rb.linearVelocity.magnitude / normalSpeed + 0.1f);
            }
        }
        else
        {
            stepTimer = 0f;
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
