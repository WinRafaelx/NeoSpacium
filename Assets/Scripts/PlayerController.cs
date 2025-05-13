using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Tuning")]
    public float forwardSpeed       = 5f;
    public float speedIncreaseRate  = 0.1f;
    public float laneDistance       = 2f;
    public float laneSmoothTime     = 0.1f;   // <— smoothing time
    public float jumpForce          = 10f;
    public float fallImpulse        = 20f;
    public float slideDuration      = 0.5f;
    public float slideScaleY        = 0.5f;

    // state
    int     currentLane;
    int     jumpsLeft;
    bool    isGrounded;
    bool    isSliding;
    Vector3 originalScale;

    // smoothing helper
    float   xVelocity;           

    // components
    Rigidbody        rb;
    GameActionInputs inputActions;
    ObstacleSpawner  spawner;

    void Awake()
    {
        inputActions = new GameActionInputs();
        inputActions.Player.MoveLeft .performed += _ => ChangeLane(-1);
        inputActions.Player.MoveRight.performed += _ => ChangeLane( 1);
        inputActions.Player.Jump     .performed += _ => TryJump();
        inputActions.Player.Slide    .performed += _ => TrySlide();
    }

    void OnEnable()  => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        rb              = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;

        originalScale  = transform.localScale;
        jumpsLeft      = 2;
        isGrounded     = true;
        spawner        = FindFirstObjectByType<ObstacleSpawner>();
    }

    void FixedUpdate()
    {
        // —— forward + accelerate ——
        Vector3 vel = rb.linearVelocity;
        vel.z = forwardSpeed;
        forwardSpeed += speedIncreaseRate * Time.fixedDeltaTime;

        // —— smooth lateral ——
        float targetX = currentLane * laneDistance;
        float newX    = Mathf.SmoothDamp(rb.position.x, targetX, ref xVelocity, laneSmoothTime);
        // Instead of forcing vel.x, we MovePosition so forward & lateral combine nicely:
        Vector3 nextPos = new Vector3(newX, rb.position.y, rb.position.z) + Vector3.forward * vel.z * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);

        // keep our rigidbody velocity aligned for jump/fall
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, vel.z);
    }

    void ChangeLane(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, -1, 1);
    }

    void TryJump()
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

    void TrySlide()
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
            jumpsLeft  = 2;
            return;
        }
        if (col.gameObject.CompareTag("Obstacle"))
        {
            rb.linearVelocity   = Vector3.zero;
            forwardSpeed  = 0;
            enabled       = false;
            spawner?.StopSpawning();
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
