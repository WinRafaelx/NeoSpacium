using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float distanceMultiplier = 1f;
    public Transform player;
    public TextMeshProUGUI scoreText;

    private float startZ;
    private float distanceScore = 0f;
    private float coinScore = 0f;

    void Awake()
    {
        Instance = this;
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
}
