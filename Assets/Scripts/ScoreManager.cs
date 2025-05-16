using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("References")]
    public Transform player;
    public TextMeshProUGUI scoreText;      // always visible
    public TextMeshProUGUI highScoreText;  // only on Game Over

    [Header("Settings")]
    public float distanceMultiplier = 1f;

    private float startZ;
    private float distanceScore = 0f;
    private float coinScore     = 0f;
    private const string HIGH_SCORE_KEY = "HighScore";

    public float GetScore()
    {
        return Mathf.Floor(distanceScore + coinScore);
    }

    void Awake()
    {
        // singleton
        Instance = this;

        // hide high-score UI until Game Over
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(false);
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        startZ = player.position.z;
    }

    void Update()
    {
        // 1) compute raw distance
        float rawDistance = (player.position.z - startZ) * distanceMultiplier;

        // 2) double it if in Danger World
        if (DangerWorldManager.Instance != null && DangerWorldManager.Instance.isInDangerWorld)
            rawDistance *= 2f;

        distanceScore = rawDistance;

        UpdateUI();
    }

    public void AddCoinScore(float amount)
    {
        // coin score is always full value (no doubling)
        coinScore += amount;
        Debug.Log("Coin score: " + coinScore);
        UpdateUI();
    }

    void UpdateUI()
    {
        float total = GetScore();
        if (scoreText != null)
            scoreText.text = "Score: " + total;
    }

    public void TrySetHighScore()
    {
        float currentHigh = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
        float finalScore = GetScore();

        // show the higher of finalScore and stored high
        if (highScoreText != null)
        {
            highScoreText.gameObject.SetActive(true);
            float display = Mathf.Max(finalScore, currentHigh);
            highScoreText.text = "High Score: " + Mathf.FloorToInt(display);
        }

        // update saved high if beaten
        if (finalScore > currentHigh)
        {
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, finalScore);
            PlayerPrefs.Save();
        }
    }
}
