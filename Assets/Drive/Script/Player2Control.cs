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
    public float maxPullableDistance = 10f; // Maximum distance the player can pull an object from
    public float pullRadius = 3f; // Radius in which the object can be pulled from
    public float maxPullSpeed = 8f;
    public float stopPullRadius = 1f; // The radius within which the object stops being pulled and starts following the player
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
    private float lowStaminaThreshold = 10f; // Threshold for low stamina (below this value, the object becomes dynamic again)


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
    private Vector3 objectInitialPosition;
    private bool isObjectFollowing = false;
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
        currentStamina = Mathf.Max(currentStamina - amount, 0); // Decrease stamina but don't let it go below 0
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
        // If stamina is 0 and the key is still held, stop pulling and release the object
        if (currentStamina <= 0 && isPulling)
        {
            StopPull();
            ReleaseObject();
            return;
        }

        // Check if the key is held down
        if (Input.GetKey(KeyCode.Keypad2)) // While holding the key
        {
            // If no pullable object is targeted
            if (targetedObject == null)
            {
                // Look for a pullable object in range
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxPullableDistance, pushPullLayer);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag(pushPullTag))
                    {
                        targetedObject = hit.gameObject;
                        objectInitialPosition = targetedObject.transform.position; // Save initial position
                        break;
                    }
                }

                // If no pullable object was found, drain stamina like Player 1
                if (targetedObject == null)
                {
                    UseStamina(staminaDepletionRate * Time.deltaTime);  // Drain stamina constantly
                }
            }

            // If an object is being pulled
            if (targetedObject != null)
            {
                // Calculate the distance from the player to the object
                float distanceToObject = Vector2.Distance(transform.position, targetedObject.transform.position);

                // If the object is within the stop radius, it starts following the player (grabbed behavior)
                if (distanceToObject <= stopPullRadius)
                {
                    if (currentStamina > lowStaminaThreshold)
                    {
                        isObjectFollowing = true;  // Start following the player if stamina is high enough
                        StopPullingObject();  // Stop pulling the object, it's now following
                    }
                    else
                    {
                        isObjectFollowing = false;  // If stamina is low, it will become dynamic again
                        ReleaseObject(); // Revert the object to dynamic behavior when stamina is low
                    }
                }
                else
                {
                    isObjectFollowing = false;  // Not within the stop pull radius yet
                    AcceleratePull();  // Continue pulling the object towards the player
                }

                // Deplete stamina over time as the ability is held
                float staminaToDeplete = staminaDepletionRate * Time.deltaTime;  // Deplete stamina constantly
                UseStamina(staminaToDeplete);  // Deplete stamina over time
            }
        }
        else if (isPulling) // When the key is released
        {
            StopPull();  // Stop pulling when the key is released
        }
        else if (isObjectFollowing) // If the object is following and the key is released
        {
            ReleaseObject();  // Release the object when the key is released
        }
    }

    void StartPull()
    {
        if (targetedObject == null)
        {
            // Look for a pullable object in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxPullableDistance, pushPullLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag(pushPullTag))
                {
                    targetedObject = hit.gameObject;
                    objectInitialPosition = targetedObject.transform.position; // Save initial position
                    break;
                }
            }
        }

        if (targetedObject != null && currentStamina > 0) // Ensure stamina is greater than 0 before starting to pull
        {
            isPulling = true;
            UseStamina(staminaDepletionRate * Time.deltaTime); // Deplete stamina initially when pulling
        }
    }

    void StopPull()
    {
        isPulling = false;
        currentStamina = Mathf.Max(currentStamina - staminaDepletionRate * Time.deltaTime, 0); // Ensure stamina is never negative
    }

    void StopPullingObject()
    {
        // Stop the object from being pulled and let it follow the player
        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.velocity = Vector2.zero;  // Stop any pulling force
            targetRb.isKinematic = true;  // Freeze the object in place (grabbed behavior)
        }
    }

    void ReleaseObject()
    {
        if (targetedObject != null)
        {
            // Release the object, stop following
            Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                targetRb.isKinematic = false; // Allow the object to move freely (dynamic behavior)
            }

            targetedObject = null; // The object is released
            isObjectFollowing = false;
        }
    }

    void AcceleratePull()
    {
        if (targetedObject == null) return;

        // Calculate the direction towards the player
        Vector2 pullDir = (transform.position - targetedObject.transform.position).normalized;

        // Move the object towards the player
        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.velocity = pullDir * maxPullSpeed;
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
