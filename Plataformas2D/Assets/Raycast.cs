using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            // Cast a ray from the camera's position to the mouse pointer's position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Create a RaycastHit variable to store information about the hit
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // The ray hit something
                // You can access information about the hit like this:
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
                Debug.Log("Hit point: " + hit.point);
            }
            else
            {
                // The ray did not hit anything
                Debug.Log("Ray did not hit anything.");
            }
        }
    }
}
    

