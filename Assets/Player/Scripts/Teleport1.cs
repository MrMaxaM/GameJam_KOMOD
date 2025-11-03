using UnityEngine;
using UnityEngine.InputSystem;

public class Teleport1 : MonoBehaviour
{
    public GameObject Player, Circle, Phone;
   
    private void OnTriggerStay2D(Collider2D Circle)
    {
        if (Circle.CompareTag("Phone"))
        {
            Debug.Log("Игрок нашел телефон.");
            if (InputSystem.actions.FindAction("Interact").IsPressed())
            {
                Player.transform.position = new Vector2(6, (float)3.0);
            }
        }
    }
}
