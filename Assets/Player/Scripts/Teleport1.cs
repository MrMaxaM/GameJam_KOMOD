using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Teleport1 : MonoBehaviour
{
    public GameObject Player, Circle, TeleportPoint;
    public SpriteRenderer sprite;
    public float lightnessBoost = 1.1f;
    public AudioClip teleportClip;       // звук телефона
    public string teleportTo;
    [Range(0f, 1f)] public float volume = 0.8f;

    private Color originalColor;
    private AudioSource audioSource;
    private bool teleporting = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (InputSystem.actions.FindAction("Interact").IsPressed() && !teleporting)
            {
                teleporting = true;
                StopCoroutine("Teleport");
                StartCoroutine("Teleport");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sprite.color = originalColor * lightnessBoost;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sprite.color = originalColor;
        }
    }

    private IEnumerator Teleport()
    {
        if (teleportClip != null)
            audioSource.PlayOneShot(teleportClip, volume);
        Fade.Instance.FadeInOut(1f);
        yield return new WaitForSeconds(1f);
        DialogueState.Instance.Teleport(teleportTo);
        Player.transform.position = TeleportPoint.transform.position;
        teleporting = false;
        yield return null;
    }
}
