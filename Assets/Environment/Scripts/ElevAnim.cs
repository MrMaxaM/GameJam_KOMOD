using UnityEngine;
using System.Collections;

public class ElevAnim : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[3];
    public float frameDuration = 0.5f;
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PlayForward();
    }

    public void PlayForward()
    {
        StartCoroutine(AnimateForward());
    }

    public void PlayBackward()
    {
        StartCoroutine(AnimateBackward());
    }

    private IEnumerator AnimateForward()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(frameDuration);
        }
    }

    private IEnumerator AnimateBackward()
    {
        for (int i = sprites.Length - 1; i >= 0; i--)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(frameDuration);
        }
    }
}