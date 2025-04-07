using UnityEngine;

public class Player2Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 12f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Platform Drop Settings")]
    public float dropTime = 0.5f;

    [Header("Air Control Settings")]
    public float airControlSpeed = 2f;
    public float momentumDamp = 5f;

    [Header("Push/Pull Settings")]
    public float pullSpeed = 5f;
    public float pushForce = 10f;
    public Transform pushOrigin; // Empty transform from chest/hand
    public float pushRadius = 1f;
    public LayerMask pushPullLayer;
    public string pushPullTag = "PushPullable";

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool isDropping;
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
    }

    void HandleMovementInput()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool canRun = isGrounded && IsTouchingAllowedRunLayer();

        // Movement Input
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;

        if (isGrounded)
        {
            float currentSpeed = (Input.GetKey(KeyCode.RightShift) && canRun) ? runSpeed : moveSpeed;
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

        // Jump
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
        int baseLayer = LayerMask.NameToLayer("Base");
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        if (hit != null)
        {
            int hitLayer = hit.gameObject.layer;
            return ((groundLayer.value & (1 << hitLayer)) != 0) || hitLayer == baseLayer;
        }
        return false;
    }
    void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) AttackA();
        if (Input.GetKeyDown(KeyCode.Keypad2)) PushForwardObject();
        if (Input.GetKeyDown(KeyCode.Keypad3)) PullNearestObject();
    }

    void AttackA()
    {
        Debug.Log("P2 Attack 1 (Numpad 1)");
        // Add your attack logic here
    }

    void PushForwardObject()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Collider2D hit = Physics2D.OverlapCircle(pushOrigin.position, pushRadius, pushPullLayer);

        if (hit != null && hit.CompareTag(pushPullTag))
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(direction * pushForce, ForceMode2D.Impulse);
            }
        }
    }


    void PullNearestObject()
    {
        if (targetedObject == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f, pushPullLayer);
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
            Vector2 dirToPlayer = (transform.position - targetedObject.transform.position).normalized;
            targetedObject.GetComponent<Rigidbody2D>().velocity = dirToPlayer * pullSpeed;
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
        Gizmos.DrawWireSphere(transform.position, 3f); // Pull range
    }
}
