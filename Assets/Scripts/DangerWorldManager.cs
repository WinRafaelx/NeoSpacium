using UnityEngine;

public class DangerWorldManager : MonoBehaviour
{
    public static DangerWorldManager Instance { get; private set; }

    [Header("Dependencies")]
    // Either assign these in the Inspector or let Awake() find them
    public PlayerController playerController;
    public ScoreManager    scoreManager;

    [Header("Settings")]
    public float initialTriggerScore = 1000f;
    public float dangerDuration      = 10f;

    public bool  isInDangerWorld { get; private set; }
    public bool  hasShield       { get; private set; }

    private float lastExitScore = 0f;
    private float dangerTimer;

    void Awake()
    {
        Instance = this;

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        // ScoreManager is a singleton, but just in case:
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;
    }

    void Update()
    {
        float currentScore = scoreManager.GetScore();

        // Trigger entry
        if (!isInDangerWorld && currentScore >= GetNextTriggerScore())
            EnterDangerWorld();

        // Count down and auto-exit
        if (isInDangerWorld)
        {
            dangerTimer -= Time.deltaTime;
            if (dangerTimer <= 0f)
                ExitDangerWorld();
        }
    }

    float GetNextTriggerScore()
    {
        return lastExitScore > 0f
            ? lastExitScore * 2f
            : initialTriggerScore;
    }

    public void EnterDangerWorld()
    {
        if (isInDangerWorld) return;

        isInDangerWorld = true;
        hasShield       = true;
        dangerTimer     = dangerDuration;

        playerController.EnableLayeredMovement(true);
        Debug.Log("🌪️ Entered Danger World");
    }

    public void ExitDangerWorld()
    {
        if (!isInDangerWorld) return;

        isInDangerWorld = false;
        hasShield       = false;

        // freeze the exact score for next trigger
        lastExitScore = scoreManager.GetScore();

        playerController.EnableLayeredMovement(false);
        Debug.Log("🚪 Exited Danger World (exitScore=" + lastExitScore + ")");
    }

    // Called from PlayerController collision logic:
    public void UseShield()
    {
        if (!hasShield) return;
        hasShield = false;
        ExitDangerWorld();
    }
}
