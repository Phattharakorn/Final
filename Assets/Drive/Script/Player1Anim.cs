using UnityEngine;

public class Player1AnimationController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    [Header("State Check")]
    public bool isGrounded;
    public bool isUsingAbility;
    public bool isWalking;
    public bool isJumping;
    public bool isFalling;

    [Header("Settings")]
    public float fallThreshold = -1f;
    public float walkSpeedThreshold = 0.1f;

    public LayerMask groundLayer;
    public LayerMask pushLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateGroundState();
        UpdateMovementState();
        UpdateJumpState();
        UpdateAbilityState();

        ApplyAnimatorParameters();
    }

    void UpdateGroundState()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                  || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, pushLayer);
        // You should update this using raycast or collision logic in your movement script
        // Example: isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    void UpdateMovementState()
    {
        isWalking = Mathf.Abs(rb.velocity.x) > walkSpeedThreshold && isGrounded;
        isFalling = rb.velocity.y < fallThreshold && !isGrounded;
    }

    void UpdateJumpState()
    {
        if (!isGrounded && rb.velocity.y > 0.1f)
        {
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }
    }

    void UpdateAbilityState()
    {
        // Example input: use this value from your player controller
        isUsingAbility = Input.GetKey(KeyCode.K);
    }

    void ApplyAnimatorParameters()
    {
        anim.SetBool("isUsingAbility", isUsingAbility);
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isJumping", isJumping);

        if (Input.GetKeyDown(KeyCode.F)) // Example key for UseAbility
        {
            anim.SetTrigger("useAbility");
        }
    }
}
