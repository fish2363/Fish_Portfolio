using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public static ResetManager Instance;
    [SerializeField] private InputReader input;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }    
        else
        {
            Destroy(gameObject);
        }
        input.OnResetKeyPressed += ResetGame;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        input.OnResetKeyPressed -= ResetGame;
    }
}
