using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour
{
    public Light2D glight;
    public Light2D playerLight;
    public float defaultPlyerLightIntencity;
    public float presentIntensity;
    public float pastIntensity = 1f;
    void Start()
    {
        glight = GetComponent<Light2D>();
        presentIntensity = glight.intensity;
        defaultPlyerLightIntencity = playerLight.intensity;

        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnTeleport += OnTeleport;
        }
    }

    void OnDestroy()
    {
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnTeleport -= OnTeleport;
        }
    }

    void OnTeleport(string tpto)
    {
        if (tpto == "past")
        {
            glight.intensity = pastIntensity;
            playerLight.intensity = 0;
        }
        else
        {
            glight.intensity = presentIntensity;
            playerLight.intensity = defaultPlyerLightIntencity;
        }
    }
}
