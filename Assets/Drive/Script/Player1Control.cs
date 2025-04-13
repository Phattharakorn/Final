using UnityEngine;
using UnityEngine.UI; // Add this for UI Slider

public class Player1Control : MonoBehaviour
{
    public AudioSource playerSource;
    public AudioClip keySound;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenerationRate = 5f;
    public float staminaDepletionRate = 20f; // Depletion when using ability
    public float staminaRecoveryDelay = 2f; // Delay before regeneration starts
    public Slider staminaSlider; // Reference to the stamina slider in the UI

    [Header("Platform Drop Settings")]
    public float dropTime = 0.5f;

    [Header("Air Control Settings")]
    public float airControlSpeed = 2f;
    public float momentumDamp = 5f;

    [Header("Push Settings")]
    public float pushForce = 10f;
    public Transform pushOrigin;
    public float pushRadius = 1f;
    public LayerMask pushLayer;
    public string pushTag = "PushPullable";

    [Header("Jump Control")]
    public float jumpCooldown = 0.1f;
    private float lastJumpTime;

    [Header("Wall Slide Settings")]
    public LayerMask wallLayer;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public float wallSlideSpeed = 2f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool isDropping;
    private bool isTouchingWall;
    private float lastStaminaUseTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        currentStamina = maxStamina; // Initialize stamina

        if (staminaSlider == null)
        {
            staminaSlider = FindObjectOfType<Slider>();
        }
        UpdateStaminaUI(); // Initialize the slider value at the start
    }

    void Update()
    {
        HandleMovementInput();
        HandlePushInput();

        RegenerateStamina();
        UpdateStaminaUI(); // Update stamina UI each frame
    }

    void HandleMovementInput()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                   || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, pushLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * transform.localScale.x, wallCheckDistance, wallLayer);

        float inputX = 0f;
        if (Input.GetKey(KeyCode.A)) inputX = -1f;
        if (Input.GetKey(KeyCode.D)) inputX = 1f;

        float currentSpeed = moveSpeed;

        if (!isGrounded && isTouchingWall && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(0, -wallSlideSpeed);
        }
        else if (isGrounded)
        {
            rb.velocity = new Vector2(inputX * currentSpeed, rb.velocity.y);
        }
        else
        {
            float targetX = inputX * airControlSpeed;
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, targetX, momentumDamp * Time.deltaTime),
                rb.velocity.y
            );
        }

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.S) && isGrounded && !isDropping)
        {
            StartCoroutine(DropThroughPlatform());
        }
    }

    System.Collections.IEnumerator DropThroughPlatform()
    {
        isDropping = true;
        PlatformEffector2D effector = GetCurrentPlatformEffector();
        if (effector != null)
        {
            playerCollider.enabled = false;
            yield return new WaitForSeconds(dropTime);
            playerCollider.enabled = true;
        }
        isDropping = false;
    }

    PlatformEffector2D GetCurrentPlatformEffector()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        return hit.collider != null ? hit.collider.GetComponent<PlatformEffector2D>() : null;
    }

    void HandlePushInput()
    {
        if (Input.GetKey(KeyCode.K) && currentStamina >= staminaDepletionRate)
        {
            playerSource.PlayOneShot(keySound);
            UseStamina(staminaDepletionRate);
            PushForwardObject();
        }
    }
    void RegenerateStamina()
    {
        // If stamina is not used recently, start regeneration
        if (Time.time - lastStaminaUseTime > staminaRecoveryDelay)
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenerationRate * Time.deltaTime, maxStamina);
        }
    }
    void UseStamina(float amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0f);
        lastStaminaUseTime = Time.time;
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            // Update the slider value based on current stamina
            staminaSlider.value = currentStamina / maxStamina;
        }
    }
    void PushForwardObject()
    {
        Collider2D hit = Physics2D.OverlapCircle(pushOrigin.position, pushRadius, pushLayer);

        if (hit != null && hit.CompareTag(pushTag))
        {
            Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, hit.transform.position);

                // Inverse of distance: closer = stronger push
                float distanceMultiplier = Mathf.Clamp01(1f - (distance / pushRadius)); // Range 0–1
                float finalPushForce = pushForce * (1f + distanceMultiplier); // Give a base + boost

                targetRb.AddForce(direction * finalPushForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pushOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pushOrigin.position, pushRadius);
        }
    }
}
