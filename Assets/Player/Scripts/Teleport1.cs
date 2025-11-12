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
    private BoxCollider2D[] colliders;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        colliders = GetComponents<BoxCollider2D>();

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
        foreach (BoxCollider2D col in colliders)
        {
            Vector2 center = col.bounds.center;
            Vector2 size = col.bounds.size;

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

            foreach (Collider2D hit in hits)
            {
                if (hit != col && hit.CompareTag("Player")) // Исключаем сам коллайдер
                {
                    Debug.Log("Не путаемся, игрок на месте");
                    return;
                }
            }
        }

        if (collision.CompareTag("Player"))
        {
            sprite.color = originalColor;
            if (teleporting)
            {
                Debug.Log("А игрок-то вышел...");
                StopCoroutine("Teleport");
                Fade.Instance.FadeFromBlack();
                teleporting = false;
            }
        }
    }

    private IEnumerator Teleport()
    {
        if (teleportClip != null)
            audioSource.PlayOneShot(teleportClip, volume);
        Fade.Instance.FadeInOut(1f);
        Debug.Log("Ждём секу...");
        yield return new WaitForSeconds(1f);

        foreach (BoxCollider2D col in colliders)
        {
            Vector2 center = col.bounds.center;
            Vector2 size = col.bounds.size;

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

            foreach (Collider2D hit in hits)
            {
                if (hit != col && hit.CompareTag("Player")) // Исключаем сам коллайдер
                {
                    Debug.Log("Игрок есть, тепаем");
                    teleporting = false;
                    DialogueState.Instance.Teleport(teleportTo);
                    Player.transform.position = TeleportPoint.transform.position;
                }
            }
        }

        teleporting = false;
        yield return null;
    }
}
