using UnityEngine;
using UnityEngine.UI; // Add this namespace for Slider

public class Player2Control : MonoBehaviour
{
    public AudioSource playerSource;
    public AudioClip pullSound;

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
    public float currentPullSpeed;
    public float pullAcceleration = 5f;
    public float pullStartDelay = 0.1f;
    public LayerMask pushPullLayer;
    public string pushPullTag = "PushPullable";
    private bool isPulling;


    [Header("Stamina Settings")]
    public float maxStamina = 100;
    public float currentStamina;
    public float staminaRegenerationRate = 5;
    public float staminaDepletionRate = 20; // Depletion when using pull
    public float staminaRecoveryDelay = 2f; // Delay before regeneration starts
    private float lowStaminaThreshold = 10; // Threshold for low stamina (below this value, the object becomes dynamic again)


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
        // Decrease stamina
        currentStamina -= amount;
        if (currentStamina < 0)  // If it goes below 0, clamp to 0
        {
            currentStamina = 0;
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
    void HandlePulling()
    {
        if (currentStamina <= 0 && isPulling)
        {
            StopPull();
            ReleaseObject();
            return;
        }

        if (Input.GetKey(KeyCode.Keypad2)) // While holding the pull key
        {
            playerSource.PlayOneShot(pullSound);
            // Center the circle around the player
            Vector2 circleCenter = transform.position;
            float radius = maxPullableDistance;  // The radius for pulling objects

            // Find all objects within the range using OverlapCircle
            Collider2D[] hits = Physics2D.OverlapCircleAll(circleCenter, radius, pushPullLayer);

            bool foundObject = false;

            // Check if any object within the radius is pullable
            foreach (var hit in hits)
            {
                if (hit.CompareTag(pushPullTag))
                {
                    targetedObject = hit.gameObject;
                    objectInitialPosition = targetedObject.transform.position;
                    foundObject = true;
                    break;
                }
            }

            // If no pullable object was found, deplete stamina
            if (!foundObject)
            {
                UseStamina(staminaDepletionRate * Time.deltaTime);
            }

            // If an object is being pulled, handle the pulling mechanics
            if (targetedObject != null)
            {
                float distanceToObject = Vector2.Distance(transform.position, targetedObject.transform.position);

                // If the object is within the stop radius, make it follow the player
                if (distanceToObject <= stopPullRadius)
                {
                    if (currentStamina > lowStaminaThreshold)
                    {
                        isObjectFollowing = true;
                        StopPullingObject();
                        // Set the object to kinematic and stop its velocity
                        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
                        if (targetRb != null)
                        {
                            targetRb.velocity = Vector2.zero;  // Set velocity to 0
                            targetRb.isKinematic = true;  // Make the object kinematic (no physics applied)
                        }
                    }
                    else
                    {
                        isObjectFollowing = false;
                        ReleaseObject();
                    }
                }
                else
                {
                    isObjectFollowing = false;
                    AcceleratePull(); // Pull the object toward the player
                }

                // Deplete stamina as the ability is held
                UseStamina(staminaDepletionRate * Time.deltaTime);
            }
        }
        else if (isPulling) // When the key is released, stop pulling
        {
            StopPull();
        }
        else if (isObjectFollowing) // If the object is following, release it
        {
            ReleaseObject();
        }
    }

    void StopPull()
    {
        isPulling = false;
        currentPullSpeed = 0f; // Reset speed when pulling stops
        currentStamina = Mathf.Max(currentStamina - staminaDepletionRate * Time.deltaTime, 0);
        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.isKinematic = false; // Allow the object to move freely (dynamic behavior)
        }
    }


    void StopPullingObject()
    {
        currentPullSpeed = 0f; // Reset speed when pulling stops
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
        currentPullSpeed = 0f; // Reset speed when pulling stops
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

        // Increase pull speed gradually over time
        currentPullSpeed = Mathf.MoveTowards(currentPullSpeed, maxPullSpeed, pullAcceleration * Time.deltaTime);

        // Calculate pull direction
        Vector2 pullDir = (transform.position - targetedObject.transform.position).normalized;

        // Apply velocity towards the player
        Rigidbody2D targetRb = targetedObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.isKinematic = false;
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
