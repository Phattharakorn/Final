using UnityEngine;
using UnityEngine.UI; // Add this namespace for Slider

public class Player2Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Platform Drop Settings")]
    public float dropTime = 0.5f;

    [Header("Air Control Settings")]
    public float airControlSpeed = 2f;
    public float momentumDamp = 5f;

    [Header("Pull Settings")]
    public float pullRadius = 3f;
    public float maxPullSpeed = 8f;
    public float pullAcceleration = 5f;
    public float pullStartDelay = 0.1f;
    public LayerMask pushPullLayer;
    public string pushPullTag = "PushPullable";

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenerationRate = 5f;
    public float staminaDepletionRate = 20f; // Depletion when using pull
    public float staminaRecoveryDelay = 2f; // Delay before regeneration starts

    [Header("Jump Control")]
    public float jumpCooldown = 0.1f;

    [Header("Wall Slide Settings")]
    public LayerMask wallLayer;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public float wallSlideSpeed = 2f;

    // Reference to the UI Slider
    public Slider staminaSlider; // Reference to the stamina slider in the UI

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool isDropping;
    private bool isTouchingWall;
    private GameObject targetedObject;
    private float currentPullSpeed;
    private bool isPulling;
    private float lastStaminaUseTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        currentStamina = maxStamina; // Initialize stamina

        // If the stamina slider isn't linked in the inspector, find it
        if (staminaSlider == null)
        {
            staminaSlider = FindObjectOfType<Slider>();
        }
        UpdateStaminaUI(); // Initialize the slider value at the start
    }

    void Update()
    {
        HandleMovementInput();
        HandlePulling();

        if (isPulling && targetedObject != null)
            AcceleratePull();

        RegenerateStamina();
        UpdateStaminaUI(); // Update stamina UI each frame
    }

    void HandleMovementInput()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                  || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, pushPullLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * transform.localScale.x, wallCheckDistance, wallLayer);

        // Movement Input (no stamina consumption here)
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;

        if (isGrounded)
        {
            float currentSpeed = moveSpeed; // No stamina needed for movement
            rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
        }
        else
        {
            float targetX = moveInput * airControlSpeed;
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, targetX, momentumDamp * Time.deltaTime),
                rb.velocity.y
            );
        }

        // Jumping (no stamina consumption)
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Drop through platform
        if (Input.GetKeyDown(KeyCode.DownArrow) && isGrounded && !isDropping)
        {
            StartCoroutine(DropThroughPlatform());
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
    void HandlePulling()
    {
        // If stamina is 0, stop pulling
        if (currentStamina <= 0)
        {
            StopPull();
            return;
        }

        if (Input.GetKey(KeyCode.Keypad2)) // While holding the key
        {
            // If no pullable object is targeted
            if (targetedObject == null)
            {
                // Look for a pullable object in range
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius, pushPullLayer);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag(pushPullTag))
                    {
                        targetedObject = hit.gameObject;
                        break;
                    }
                }

                // If no pullable object was found, drain stamina like Player 1
                if (targetedObject == null)
                {
                    UseStamina(staminaDepletionRate * Time.deltaTime);  // Drain stamina constantly
                }
            }

            // If pulling a targeted object, continue pulling
            if (targetedObject != null)
            {
                AcceleratePull();  // Pull the object
                float staminaToDeplete = staminaDepletionRate * Time.deltaTime;  // Deplete stamina while pulling
                UseStamina(staminaToDeplete);  // Deplete stamina over time
            }
        }
        else if (isPulling) // When the key is released
        {
            StopPull();  // Stop pulling when the key is released
        }
    }

    void StartPull()
    {
        if (targetedObject == null)
        {
            // Look for a pullable object in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius, pushPullLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag(pushPullTag))
                {
                    targetedObject = hit.gameObject;
                    break;
                }
            }
        }

        if (targetedObject != null && currentStamina > 0) // Ensure stamina is greater than 0 before starting to pull
        {
            isPulling = true;
            currentPullSpeed = 0f;
            UseStamina(staminaDepletionRate * Time.deltaTime); // Deplete stamina initially when pulling
        }
    }

    void StopPull()
    {
        isPulling = false;
        currentPullSpeed = 0f;
        targetedObject = null;
    }

    void AcceleratePull()
    {
        if (targetedObject == null) return;

        currentPullSpeed = Mathf.Min(currentPullSpeed + pullAcceleration * Time.deltaTime, maxPullSpeed);
        Vector2 pullDir = (transform.position - targetedObject.transform.position).normalized;

        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.velocity = pullDir * currentPullSpeed;
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            // Update the slider value based on current stamina
            staminaSlider.value = currentStamina / maxStamina;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
