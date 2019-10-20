using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneAt : MonoBehaviour
{
    public float delay = 1f;
    public string sceneName;
    void Start()
    {
        Invoke("LoadScene",delay);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
