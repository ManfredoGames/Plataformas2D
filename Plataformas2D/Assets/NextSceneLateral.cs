using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSceneLateral : MonoBehaviour
{
    public string SceneToLoad;
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector3 newPosition = collision.transform.localPosition;
            newPosition.x += 1;
            collision.transform.localPosition = newPosition;
            LevelLoaderScript.instance.LevelLoader(SceneToLoad);
        }
    }
}
