using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float distanceMultiplier = 1f;
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private float startZ;
    private float distanceScore = 0f;
    private float coinScore = 0f;
    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        Instance = this;
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(false); // ❌ hide at start
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        startZ = player.position.z;
    }

    void Update()
    {
        distanceScore = (player.position.z - startZ) * distanceMultiplier;
        UpdateUI();
    }

    public void AddCoinScore(float amount)
    {
        coinScore += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        float total = Mathf.Floor(distanceScore + coinScore);
        if (scoreText != null)
            scoreText.text = "Score: " + total;
    }

    public void TrySetHighScore()
    {
        float currentHigh = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0);

        float score = Mathf.Floor(distanceScore + coinScore);
        // 👇 always show current high score at game over
        if (highScoreText != null)
        {
            highScoreText.gameObject.SetActive(true);
            float displayValue = Mathf.Max(score, currentHigh);
            highScoreText.text = "High Score: " + Mathf.FloorToInt(displayValue);
        }

        // update storage only if new high
        if (score > currentHigh)
        {
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }
}
