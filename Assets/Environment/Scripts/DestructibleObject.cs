using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public GameObject destroyedVersion;
    
    public void DestroyObject()
    {
        if (destroyedVersion != null)
        {
            Instantiate(destroyedVersion, transform.position, transform.rotation);
        }
        
        Destroy(gameObject);
    }
}
