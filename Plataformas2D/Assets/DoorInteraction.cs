using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    public GameObject interactionObject;
    public string sceneToLoad;

    private bool isPlayerInRange = false;

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && isPlayerInRange)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Asegúrate de que tu jugador tenga la etiqueta "Player"
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
