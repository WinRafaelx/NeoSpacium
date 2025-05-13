using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // ─── Public Tuning ───────────────────
    public float forwardSpeed      = 5f;
    public float speedIncreaseRate = 0.1f;
    public float laneDistance      = 2f;
    public float jumpForce         = 10f;
    public float fallImpulse       = 20f;
    public float slideDuration     = 0.5f;
    public float slideScaleY       = 0.5f;

    // ─── State ─────────────────────────
    private int     currentLane;
    private int     jumpsLeft;
    private bool    isGrounded;
    private bool    isSliding;
    private Vector3 originalScale;

    // ─── Components ────────────────────
    private Rigidbody        rb;
    private GameActionInputs inputActions;
    private ObstacleSpawner  spawner;

    void Awake()
    {
        inputActions = new GameActionInputs();
        inputActions.Player.MoveLeft .performed += _ => ChangeLane(-1);
        inputActions.Player.MoveRight.performed += _ => ChangeLane(1);
        inputActions.Player.Jump     .performed += _ => TryJump();
        inputActions.Player.Slide    .performed += _ => TrySlide();
    }

    void OnEnable()  => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;

        originalScale = transform.localScale;
        jumpsLeft     = 2;
        isGrounded    = true;

        // Use the new API
        spawner = FindFirstObjectByType<ObstacleSpawner>();
    }

    void FixedUpdate()
    {
        // 1) Forward + acceleration
        Vector3 vel = rb.linearVelocity;
        vel.z = forwardSpeed;
        forwardSpeed += speedIncreaseRate * Time.fixedDeltaTime;

        // 2) Smooth lane X
        float targetX = currentLane * laneDistance;
        float deltaX  = (targetX - rb.position.x) * 10f;
        vel.x = deltaX;

        rb.linearVelocity = vel;      // <-- use .velocity, not .linearVelocity
    }

    void ChangeLane(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, -1, 1);
    }

    void TryJump()
    {
        if (jumpsLeft > 0)
        {
            var vel = rb.linearVelocity;
            vel.y     = 0f;
            rb.linearVelocity = vel;    // reset vertical before jump

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsLeft--;
            isGrounded = false;
        }
    }

    void TrySlide()
    {
        if (isSliding) return;
        isSliding = true;

        // fast fall if airborne, but only if not already too fast
        if (!isGrounded && rb.linearVelocity.y > -fallImpulse)
            rb.AddForce(Vector3.down * fallImpulse, ForceMode.Impulse);

        // squash
        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * slideScaleY,
            originalScale.z
        );
        Invoke(nameof(EndSlide), slideDuration);
    }

    void EndSlide()
    {
        transform.localScale = originalScale;
        isSliding = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground") && col.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            jumpsLeft  = 2;
            return;
        }

        if (col.gameObject.CompareTag("Obstacle"))
        {
            // GAME OVER
            rb.linearVelocity     = Vector3.zero;
            forwardSpeed    = 0f;
            enabled         = false;
            spawner?.StopSpawning();   // now calls into your ObstacleSpawner
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
