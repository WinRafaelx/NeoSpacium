using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject slideObstaclePrefab;
    public GameObject jumpObstaclePrefab;
    public GameObject doubleJumpObstaclePrefab;
    public GameObject coinPrefab;
    public Transform player; // drag in your Player here

    [Header("Grid Settings")]
    public float segmentLength = 17f;
    public int segmentsAhead = 10;
    public float despawnOffset = 5f;
    public float laneDistance = 2f; // ← you missed this line

    public float startSafeZoneLength = 10f;

    private int segmentToSpawn = 0;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        Update();
    }

    void Update()
    {
        int playerSlice = Mathf.FloorToInt(player.position.z / segmentLength);

        while (segmentToSpawn < playerSlice + segmentsAhead)
        {
            float sliceZ = segmentToSpawn * segmentLength;

            // 🛑 Skip spawning in the initial safe zone
            if (sliceZ >= startSafeZoneLength)
            {
                SpawnSlice(segmentToSpawn);
            }

            segmentToSpawn++;
        }

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            var obs = activeObstacles[i];
            if (player.position.z - obs.transform.position.z > despawnOffset)
            {
                Destroy(obs);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void SpawnSlice(int sliceIndex)
    {
        int obstaclesInThisSlice = Random.Range(1, 3); // 1 or 2
        List<int> usedLanes = new List<int>();

        for (int i = 0; i < obstaclesInThisSlice; i++)
        {
            int lane;
            do lane = Random.Range(-1, 2);
            while (usedLanes.Contains(lane)); // avoid same lane
            usedLanes.Add(lane);

            float zPos = sliceIndex * segmentLength + Random.Range(2f, segmentLength - 2f);

            // Choose prefab
            float randomType = Random.value;
            GameObject prefab;
            float obstacleY;

            if (randomType < 0.2f)
            {
                prefab = jumpObstaclePrefab;
                obstacleY = 0.5f;
            }
            else if (randomType < 0.6f)
            {
                prefab = slideObstaclePrefab;
                obstacleY = 3.5f;
            }
            else
            {
                prefab = doubleJumpObstaclePrefab;
                obstacleY = 2f;
            }

            Vector3 obsPos = new Vector3(lane * laneDistance, obstacleY, zPos);
            var obs = Instantiate(prefab, obsPos, Quaternion.identity);
            activeObstacles.Add(obs);

            // 🎯 Coin logic — attach coin to obstacle by type (random chance)
            if (Random.value < 0.6f) // 50% chance to spawn coin with obstacle
            {
                Vector3 coinPos = obsPos;

                if (prefab == slideObstaclePrefab)
                {
                    coinPos.y = 1.5f; // slide height
                }
                else if (prefab == doubleJumpObstaclePrefab)
                {
                    coinPos.y += 4f; // place above the obstacle
                }
                else
                {
                    coinPos.y += 2f;
                }

                Instantiate(coinPrefab, coinPos, Quaternion.identity);
            }
        }
    }


    public void StopSpawning()
    {
        enabled = false;
    }
}
