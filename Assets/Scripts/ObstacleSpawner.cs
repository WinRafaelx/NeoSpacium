using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject obstaclePrefab;  // your red cube prefab
    public Transform player;          // drag in your Player here

    [Header("Grid Settings")]
    public float segmentLength = 10f;  // each “slice” of road
    public int segmentsAhead = 10;  // how many slices we keep ahead
    public float despawnOffset = 5f;   // remove obstacles 5 units behind

    // internal
    private int segmentToSpawn = 0;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        // fill the first few slices at startup
        Update();
    }

    void Update()
    {
        // 1) figure out which slice the player is in
        int playerSlice = Mathf.FloorToInt(player.position.z / segmentLength);

        // 2) spawn all slices up to (playerSlice + segmentsAhead)
        while (segmentToSpawn < playerSlice + segmentsAhead)
        {
            SpawnSlice(segmentToSpawn);
            segmentToSpawn++;
        }

        // 3) clean up any obstacle behind the player
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
        // spawn 1–3 obstacles in this slice
        int count = Random.Range(1, 4);
        for (int i = 0; i < count; i++)
        {
            int lane = Random.Range(-1, 2);  // -1, 0, or 1
            float zPos = sliceIndex * segmentLength
                       + Random.Range(0f, segmentLength);

            Vector3 pos = new Vector3(lane * 2f, 0.5f, zPos);
            var obs = Instantiate(obstaclePrefab, pos, Quaternion.identity);
            activeObstacles.Add(obs);
        }
    }
    public void StopSpawning()
    {
        enabled = false;
    }

}
