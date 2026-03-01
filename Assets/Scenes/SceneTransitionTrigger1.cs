using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger1 : MonoBehaviour
{
    public string targetSceneName;
    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            SceneManager.LoadScene(targetSceneName);
    }
}
