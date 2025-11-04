using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Teleport1 : MonoBehaviour
{
    public GameObject Player, Circle, TeleportPoint;
    public SpriteRenderer sprite;
    public float lightnessBoost = 1.1f;
    private Color originalColor;

    void Start()
    {
        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (InputSystem.actions.FindAction("Interact").IsPressed())
            {
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
        Fade.Instance.FadeInOut(1f);
        yield return new WaitForSeconds(1f);
        Player.transform.position = TeleportPoint.transform.position;
        yield return null;
    }
}
