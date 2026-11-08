using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central game state: score, key progress, win/lose panels, restart, and UI feedback.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI informationText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Beginner Settings")]
    [SerializeField, Min(1)] private int startingLives = 3;
    [SerializeField, Min(1f)] private float timeLimit = 60f;
    [SerializeField, Min(0.1f)] private float hitProtectionSeconds = 1.5f;

    [Header("Starting Message")]
    [SerializeField] private string startMessage = "Find the key and escape!";

    private int score;
    private bool hasKey;
    private bool isPlaying = true;
    private int lives;
    private float timeRemaining;
    private float nextDamageTime;

    public bool IsPlaying => isPlaying;
    public bool HasKey => hasKey;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GetComponent<UiAnimator>() == null)
        {
            gameObject.AddComponent<UiAnimator>();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPlaying = true;
        hasKey = false;
        score = 0;
        lives = Mathf.Max(1, startingLives);
        timeRemaining = Mathf.Max(1f, timeLimit);
        nextDamageTime = 0f;
        UpdateLivesUI();
        UpdateTimerUI();

        UpdateScoreUI();
        ShowMessage(startMessage);

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        // Delta time makes the countdown independent of the frame rate.
        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        UpdateTimerUI();
        if (timeRemaining <= 0f)
        {
            TriggerGameOver();
            ShowMessage("Time's up!");
        }
    }

    public void DamagePlayer(PlayerMovement player)
    {
        // Ignore repeated contacts briefly after a hit.
        if (!isPlaying || player == null || Time.time < nextDamageTime) return;

        nextDamageTime = Time.time + hitProtectionSeconds;
        lives--;
        UpdateLivesUI();
        if (lives <= 0)
        {
            TriggerGameOver();
            return;
        }

        player.Respawn();
        ShowMessage("Ouch! Back to the start. Lives left: " + lives);
    }

    private void UpdateLivesUI()
    {
        if (livesText != null) livesText.text = "Lives: " + lives;
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining) + "s";
        timerText.color = timeRemaining <= 10f ? Color.red : Color.white;
    }

    public void CollectKey(int points)
    {
        if (!isPlaying)
        {
            return;
        }

        // Play SFX first while the game is still running at normal time scale.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKeyCollect();
        }

        hasKey = true;
        AddScore(points);
        ShowMessage("Key collected! Find the exit!");
    }

    public void AddScore(int points)
    {
        if (!isPlaying) return;
        score += points;
        UpdateScoreUI();

        if (UiAnimator.Instance != null && scoreText != null)
        {
            UiAnimator.Instance.PunchScale(scoreText.transform, 0.35f, 0.3f);
        }
    }

    public void ShowMessage(string message)
    {
        if (informationText == null)
        {
            return;
        }

        if (UiAnimator.Instance != null)
        {
            UiAnimator.Instance.FlashMessage(informationText, message);
        }
        else
        {
            informationText.text = message;
        }
    }

    public void TriggerWin()
    {
        if (!isPlaying)
        {
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }

        EndGame();
        ShowMessage("YOU WIN!");

        if (UiAnimator.Instance != null)
        {
            UiAnimator.Instance.ShowPanelAnimated(winPanel);
        }
        else if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void TriggerGameOver()
    {
        if (!isPlaying)
        {
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        EndGame();
        ShowMessage("GAME OVER!");

        if (UiAnimator.Instance != null)
        {
            UiAnimator.Instance.ShowPanelAnimated(gameOverPanel);
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void EndGame()
    {
        isPlaying = false;
        // Pause gameplay, but keep UI / audio animations on unscaled time.
        Time.timeScale = 0f;

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            if (UiAnimator.Instance != null)
            {
                UiAnimator.Instance.PunchScale(restartButton.transform, 0.2f, 0.35f);
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }
}
