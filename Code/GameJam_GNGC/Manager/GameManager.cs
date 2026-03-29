using Ami.BroAudio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputReader InputReader;
    public static GameManager Instance;
    private BlackHole player;

    [Header("스코어 텍스트")]
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [field:SerializeField]
    public int CurrentScore { get; set; }

    [Header("레벨업하는 스코어 단위")]
    public float LevelUpScore =1000;

    [SerializeField]
    private CanvasGroup gameOverUI;

    private bool isShaking;
    private bool isDeath;

    [Header("오디오")]
    [SerializeField]
    private SoundID bgm;
    [SerializeField]
    private SoundID gameOver;

    private void Awake()
    {
        Instance = this;
        player = FindAnyObjectByType<BlackHole>();
        InputReader.OnMainMenuKeyPressed += ReStartScene;
        InputReader.OnReStartKeyPressed += ReScene;
    }
    private void OnDisable()
    {
        InputReader.OnMainMenuKeyPressed -= ReStartScene;
        InputReader.OnReStartKeyPressed -= ReScene;
    }
    private void Start()
    {
        scoreText.text = $"{CurrentScore}";
        BroAudio.Play(bgm);
    }

    public void ReScene()
    {
        if (!isDeath) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void ReStartScene()
    {
        if (!isDeath) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void Score(int score)
    {
        CurrentScore += score;
        SetText();

        int expectedLevel = Mathf.FloorToInt(CurrentScore / LevelUpScore);
        if (player.Level <= expectedLevel)
        {
            player.PlusLevel();
        }
    }
    public void GameOver()
    {
        gameOverUI.DOFade(1f, 0.2f).OnComplete(() => {
            BroAudio.Play(gameOver);
            BroAudio.Stop(bgm);
            gameOverUI.blocksRaycasts = true;
            gameOverUI.interactable = true;
            isDeath = true;
            Time.timeScale = 0f;
        }
        );
    }

    void SetText()
    {
        scoreText.text = $"{CurrentScore}";
        if(!isShaking)
        {
            isShaking = true;
            scoreText.GetComponent<RectTransform>().DOShakeRotation(0.2f, 20f, 10, 10f).OnComplete(() => isShaking = false);
        }
    }
}
