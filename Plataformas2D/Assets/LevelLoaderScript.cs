using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderScript : MonoBehaviour
{
    public static LevelLoaderScript instance;

    public Animator animator;

    public float TransTime = 1f;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void LevelLoader(string SceneName)
    {
        StartCoroutine(LOADLEVEL(SceneName));
    }

    IEnumerator LOADLEVEL(string SceneName)
    {
        animator.SetTrigger("Start");

        yield return new WaitForSeconds(TransTime);

        SceneManager.LoadScene(SceneName);
    }
}
