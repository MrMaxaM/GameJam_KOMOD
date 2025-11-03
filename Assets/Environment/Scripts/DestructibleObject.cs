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

        Closet closet = GetComponent<Closet>();
        if (closet != null)
        {
            closet.ForceExit();
        }
        
        Destroy(gameObject);
    }
}
