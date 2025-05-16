using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Tuning")]
    public float forwardSpeed = 10f;
    public float speedIncreaseRate = 0.2f;
    public float laneDistance = 2f;
    public float laneSmoothTime = 0.1f;   // <— smoothing time
    public float jumpForce = 10f;
    public float fallImpulse = 20f;
    public float slideDuration = 0.5f;
    public float slideScaleY = 0.5f;

    private bool layeredMode = false;
    private int currentLayerY = 1;   // 0 = bottom, 1 = mid, 2 = top
    public float layerHeight = 2f;
    private float yVelocity;

    private float verticalVelocity = 0f;
    private const float GRAVITY = -20f;

    public void EnableLayeredMovement(bool on)
    {
        layeredMode = on;
        currentLayerY = 1; // reset to middle
    }



    // state
    int currentLane;
    int jumpsLeft;
    bool isGrounded;
    bool isSliding;
    Vector3 originalScale;

    // smoothing helper
    float xVelocity;

    // components
    Rigidbody rb;
    GameActionInputs inputActions;
    ObstacleSpawner spawner;

    void Awake()
    {
        inputActions = new GameActionInputs();
        inputActions.Player.MoveLeft.performed += _ => ChangeLane(-1);
        inputActions.Player.MoveRight.performed += _ => ChangeLane(1);
        inputActions.Player.Jump.performed += _ => TryJump();
        inputActions.Player.Slide.performed += _ => TrySlide();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        originalScale = transform.localScale;
        jumpsLeft = 2;
        isGrounded = true;
        spawner = FindFirstObjectByType<ObstacleSpawner>();
    }

    void FixedUpdate()
    {
        forwardSpeed += speedIncreaseRate * Time.fixedDeltaTime;

        float targetX = currentLane * laneDistance;
        float newX = Mathf.SmoothDamp(rb.position.x, targetX, ref xVelocity, laneSmoothTime);

        float newY = transform.position.y;
        if (layeredMode)
        {
            float targetY = currentLayerY * layerHeight;
            newY = Mathf.SmoothDamp(rb.position.y, targetY, ref yVelocity, laneSmoothTime);
        }

        Vector3 nextPos = new Vector3(newX, newY, rb.position.z + forwardSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);

    }

    void ChangeLane(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, -1, 1);
    }

    void TryJump()
    {
        if (layeredMode)
        {
            if (currentLayerY < 2) currentLayerY++;
        }
        else
        {
            if (jumpsLeft > 0)
            {
                var v = rb.linearVelocity;
                v.y = 0; rb.linearVelocity = v;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpsLeft--;
                isGrounded = false;
            }
        }

    }

    void TrySlide()
    {
        if (layeredMode)
        {
            if (currentLayerY > 0) currentLayerY--;
        }
        else
        {
            if (isSliding) return;
            isSliding = true;

            // fall faster in air
            if (!isGrounded && rb.linearVelocity.y > -fallImpulse)
                rb.AddForce(Vector3.down * fallImpulse, ForceMode.Impulse);

            // squash down
            transform.localScale = new Vector3(
                originalScale.x,
                originalScale.y * slideScaleY,
                originalScale.z
            );
            Invoke(nameof(EndSlide), slideDuration);
        }
    }

    void EndSlide()
    {
        transform.localScale = originalScale;
        isSliding = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground") && col.contacts[0].normal.y > .5f)
        {
            isGrounded = true;
            jumpsLeft = 2;
            return;
        }
        if (col.gameObject.CompareTag("Obstacle"))
        {
            if (DangerWorldManager.Instance.isInDangerWorld && DangerWorldManager.Instance.hasShield)
            {
                DangerWorldManager.Instance.UseShield();
                return; // survive
            }

            rb.linearVelocity = Vector3.zero;
            forwardSpeed = 0;
            enabled = false;
            spawner?.StopSpawning();

            ScoreManager.Instance.TrySetHighScore();
            GameManager.Instance.GameOver();
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
