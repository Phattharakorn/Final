using UnityEngine;

public class Player1Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 12f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public LayerMask baseLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Platform Drop Settings")]
    public float dropTime = 0.5f;

    [Header("Air Control Settings")]
    public float airControlSpeed = 2f;
    public float momentumDamp = 5f;

    [Header("Push/Pull Settings")]
    public float pullRadius = 3f; // 👈 Add this
    public float pullSpeed = 5f;
    public float pushForce = 10f;
    public Transform pushOrigin; // Empty transform from chest/hand
    public float pushRadius = 1f;
    public LayerMask pushPullLayer;
    public string pushPullTag = "PushPullable";


    [Header("Advanced Pull Settings")]
    public float maxPullSpeed = 8f;
    public float pullAcceleration = 5f;

    private float currentPullSpeed = 0f;
    private bool isPulling = false;

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
    private GameObject targetedObject;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleAttackInput();

        if (isPulling && targetedObject != null)
        {
            AcceleratePull();
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            StopPull();
        }
    }


    void HandleMovementInput()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
            || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, pushPullLayer)
            || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, baseLayer);


        bool canRun = isGrounded && IsTouchingAllowedRunLayer();

        // Wall check (assumes wallCheck is to the side of the player, facing localScale.x direction)
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * transform.localScale.x, wallCheckDistance, wallLayer);

        // Movement Input
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        // Wall slipping logic
        if (!isGrounded && isTouchingWall && rb.velocity.y < 0)
        {
            // Slowly slide down the wall
            rb.velocity = new Vector2(0, -wallSlideSpeed);
        }
        else if (isGrounded)
        {
            // On ground: normal or running speed
            float currentSpeed = (Input.GetKey(KeyCode.LeftShift) && canRun) ? runSpeed : moveSpeed;
            rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
        }
        else
        {
            // In air: gradually adjust horizontal movement
            float targetX = moveInput * airControlSpeed;
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, targetX, momentumDamp * Time.deltaTime),
                rb.velocity.y
            );
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Drop through platform
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

    bool IsTouchingAllowedRunLayer()
    {
        // Convert layer names to layer indices
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        int baseLayerIndex = LayerMask.NameToLayer("Base");

        // Detect the collider under the player
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        if (hit != null)
        {
            int hitLayer = hit.gameObject.layer;

            // Check if hit layer is either "Ground" or "Base"
            return hitLayer == groundLayerIndex || hitLayer == baseLayerIndex;
        }

        return false;
    }

    void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartPull();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            PushForwardObject();
        }
    }

    void Attack()
    {
        Debug.Log("Attack Function A (J)");
        // Add your attack logic here
    }
    void StartPull()
    {
        if (targetedObject == null)
        {
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

        if (targetedObject != null)
        {
            isPulling = true;
            currentPullSpeed = 0f;
        }
    }

    void AcceleratePull()
    {
        currentPullSpeed = Mathf.Min(currentPullSpeed + pullAcceleration * Time.deltaTime, maxPullSpeed);
        Vector2 pullDir = (transform.position - targetedObject.transform.position).normalized;

        Rigidbody2D rb = targetedObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = pullDir * currentPullSpeed;
        }
    }

    void StopPull()
    {
        isPulling = false;
        currentPullSpeed = 0f;
        targetedObject = null;
    }

    void PushForwardObject()
    {
        Collider2D hit = Physics2D.OverlapCircle(pushOrigin.position, pushRadius, pushPullLayer);

        if (hit != null && hit.CompareTag(pushPullTag))
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                rb.AddForce(direction * pushForce, ForceMode2D.Impulse);
            }
        }
    }


    // Visual Debug for Push/Pull Range
    void OnDrawGizmosSelected()
    {
        if (pushOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pushOrigin.position, pushRadius);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius); // Pull range

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 3f); // Pull range
    }
}
