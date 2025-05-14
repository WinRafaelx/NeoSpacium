using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject slideObstaclePrefab;
    public GameObject jumpObstaclePrefab;
    public GameObject doubleJumpObstaclePrefab;
    public GameObject coinPrefab;
    public Transform player;                   // drag in your Player here
    public PlayerController playerController;  // link your PlayerController

    [Header("Spawn Distances")]
    public float lookaheadDistance = 50f;    // how far ahead to fill
    public float minGap = 10f;    // never closer than this
    public float baseReactionTime = 1.0f;   // slowest reaction
    public float minReactionTime = 0.5f;   // fastest reaction
    public float maxSpeedForDifficulty = 50f;  // when reaction is min
    public float randomGapVariance = 1.2f;   // up to 20% extra gap

    [Header("Two-Lane Block")]
    [Range(0, 1)] public float twoLaneBlockMinChance = 0.1f;
    [Range(0, 1)] public float twoLaneBlockMaxChance = 0.4f;

    [Header("Cleanup")]
    public float despawnOffset = 5f;          // destroy when this far behind

    private float nextSpawnZ;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        // start spawning ahead of player
        nextSpawnZ = player.position.z + lookaheadDistance;
        // prefill all the way to lookaheadDistance
        while (player.position.z + lookaheadDistance > nextSpawnZ)
            SpawnOne();
    }

    void Update()
    {
        // keep filling ahead
        while (player.position.z + lookaheadDistance > nextSpawnZ)
            SpawnOne();

        // cleanup old obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activeObstacles[i].transform.position.z > despawnOffset)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void SpawnOne()
    {
        // compute difficulty [0→1]
        float speed = playerController.forwardSpeed;
        float diff = Mathf.InverseLerp(0f, maxSpeedForDifficulty, speed);

        // reaction time shrinks as diff↑
        float reaction = Mathf.Lerp(baseReactionTime, minReactionTime, diff);
        // gap = max(minGap, speed×reaction) with a bit of randomness
        float gap = Mathf.Max(minGap, speed * reaction) * Random.Range(1f, randomGapVariance);

        // compute block chances
        float twoLaneChance = Mathf.Lerp(twoLaneBlockMinChance, twoLaneBlockMaxChance, diff);
        float threeLaneChance = Mathf.Lerp(0f, 0.1f, diff);  // up to 10% at max speed

        // 1) three-lane block?
        if (Random.value < threeLaneChance)
        {
            SpawnObstacleInLane(-1, nextSpawnZ);
            SpawnObstacleInLane(0, nextSpawnZ);
            SpawnObstacleInLane(1, nextSpawnZ);
        }
        // 2) two-lane block?
        else if (Random.value < twoLaneChance)
        {
            var lanes = new List<int> { -1, 0, 1 };
            int first = lanes[Random.Range(0, lanes.Count)];
            lanes.Remove(first);
            int second = lanes[Random.Range(0, lanes.Count)];
            SpawnObstacleInLane(first, nextSpawnZ);
            SpawnObstacleInLane(second, nextSpawnZ);
        }
        // 3) single lane
        else
        {
            int lane = Random.Range(-1, 2);
            SpawnObstacleInLane(lane, nextSpawnZ);
        }

        nextSpawnZ += gap;
    }

    void SpawnObstacleInLane(int lane, float zPos)
    {
        // pick obstacle type
        float r = Random.value;
        GameObject prefab;
        float y;
        if (r < 0.2f) { prefab = jumpObstaclePrefab; y = 0.5f; }
        else if (r < 0.6f) { prefab = slideObstaclePrefab; y = 3.5f; }
        else { prefab = doubleJumpObstaclePrefab; y = 2f; }

        // spawn it
        Vector3 pos = new Vector3(lane * playerController.laneDistance, y, zPos);
        var obs = Instantiate(prefab, pos, Quaternion.identity);
        activeObstacles.Add(obs);

        // coin on top (50% chance)
        if (Random.value < 0.5f)
        {
            Vector3 coinPos = pos;
            if (prefab == slideObstaclePrefab) coinPos.y = 0.5f;
            else if (prefab == doubleJumpObstaclePrefab) coinPos.y += 4f;
            else coinPos.y += 2f;
            Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Stops any further spawning (e.g. on Game Over)
    /// </summary>
    public void StopSpawning()
    {
        enabled = false;
    }
}
