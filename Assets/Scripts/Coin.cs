using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Tooltip("How far behind the player before this coin auto‐destroys")]
    public float cleanupDistance = 5f;

    Transform         playerTf;
    PlayerController  playerCtrl;

    void Start()
    {
        // 1) Find the player once
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            playerTf   = go.transform;
            playerCtrl = go.GetComponent<PlayerController>();
        }

        // 2) Enforce trigger on our collider
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        // 3) Spin the coin
        transform.Rotate(0, 200f * Time.deltaTime, 0, Space.World);

        // 4) Auto‐destroy if we fall behind
        if (playerTf != null && playerTf.position.z - transform.position.z > cleanupDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 5) Determine speed:
        //    Prefer forwardSpeed from your controller (always nonzero while running)
        float speed = 0f;
        if (playerCtrl != null)
        {
            speed = playerCtrl.forwardSpeed;
        }
        else if (other.attachedRigidbody != null)
        {
            speed = other.attachedRigidbody.linearVelocity.z;
        }

        Debug.Log($"Coin picked up at speed {speed:F1}");

        // 6) Award points
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddCoinScore(speed * 10f);

        Destroy(gameObject);
    }
}
