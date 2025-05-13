using UnityEngine;
using System.Collections.Generic;

public class InfiniteGround : MonoBehaviour
{
    public Transform player;
    public GameObject groundPrefab;
    public int tilesAhead = 3;
    public float tileLength = 20f;

    private List<GameObject> tiles = new List<GameObject>();

    void Start()
    {
        // Initial tiles
        for (int i = 0; i < tilesAhead; i++)
            SpawnTile(i * tileLength);
    }

    void Update()
    {
        var firstTile = tiles[0];
        
        if (player.position.z - firstTile.transform.position.z > tileLength)
        {
            // Move behind tile forward
            firstTile.transform.position += Vector3.forward * tileLength * tilesAhead;

            // Rotate the list
            tiles.RemoveAt(0);
            tiles.Add(firstTile);
        }
    }

    void SpawnTile(float zPos)
    {
        GameObject tile = Instantiate(groundPrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        tiles.Add(tile);
    }
}
