using UnityEngine;

public class Coin : MonoBehaviour
{
    public Transform player;
    public float cleanupDistance = 5f;

    void Start()
    {
        // Find player once
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        transform.Rotate(0, 200f * Time.deltaTime, 0);

        // Auto-destroy if far behind player
        if (player != null && player.position.z - transform.position.z > cleanupDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float playerSpeed = Mathf.Max(0f, rb.linearVelocity.z); // ensure non-negative
                ScoreManager.Instance.AddCoinScore(playerSpeed * 10f);
            }

            Destroy(gameObject);
        }
    }
}
