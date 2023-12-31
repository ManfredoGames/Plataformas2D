using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemInteraction : MonoBehaviour
{
    public GameObject interactionObject;
    private bool isPlayerInRange = false;

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && isPlayerInRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Aseg�rate de que tu jugador tenga la etiqueta "Player"
        {
            isPlayerInRange = true;
            interactionObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            interactionObject.SetActive(false);
        }
    }
}
