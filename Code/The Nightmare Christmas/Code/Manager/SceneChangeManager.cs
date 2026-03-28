using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    private AsyncOperation asyncOperation;
    public string deathToGame_SceneName = "KYHMap";

    public static SceneChangeManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        asyncOperation = SceneManager.LoadSceneAsync("DeathScene");
        asyncOperation.allowSceneActivation = false;
    }

    public void DeathScene()
    {
        SceneManager.LoadScene("DeathScene");
    }
}
