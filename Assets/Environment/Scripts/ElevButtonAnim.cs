using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevButtonAnim : MonoBehaviour
{
    public Image[] images = new Image[2];
    public float frameDuration = 0.5f;
    
    private Image imageRenderer;

    void Start()
    {
        imageRenderer = GetComponent<Image>();
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
        for (int i = 0; i < images.Length; i++)
        {
            imageRenderer = images[i];
            imageRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(frameDuration);
        }
    }

    private IEnumerator AnimateBackward()
    {
        for (int i = images.Length - 1; i >= 0; i--)
        {
            imageRenderer = images[i];
            imageRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(frameDuration);
        }
    }
}