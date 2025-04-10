using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player2DAnimator : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();
        FlipCharacter();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        anim.SetBool("isWalking", moveInput != 0);
    }

    void HandleJumpAndDrop()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }
        else
        {
            anim.SetBool("isJumping", !isGrounded);
        }

        if (Input.GetKey(KeyCode.S) && !isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, -Mathf.Abs(jumpForce)); // Drop faster
            anim.SetBool("isDropping", true);
        }
        else
        {
            anim.SetBool("isDropping", false);
        }
    }

    void HandleAction()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.SetTrigger("Action1");
        }
    }

    void FlipCharacter()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if ((moveInput > 0 && !facingRight) || (moveInput < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
    }
}
