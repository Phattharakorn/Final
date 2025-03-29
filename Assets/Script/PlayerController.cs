using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;

    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    private bool isGrounded;

    private int playerIndex; // To differentiate Player 1 and Player 2

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerIndex = playerInput.playerIndex; // Automatically assigned by Unity

        Debug.Log($"Player {playerIndex + 1} Initialized with {playerInput.devices[0].displayName}");
    }

    private void OnEnable()
    {
        // Assign input actions
        playerInput.actions["Move_Controller"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move_Controller"].canceled += ctx => moveInput = Vector2.zero;

        playerInput.actions["Jump"].performed += ctx => Jump();
        playerInput.actions["Attack"].performed += ctx => Attack();
        playerInput.actions["Push"].performed += ctx => Push();
        playerInput.actions["Pull"].performed += ctx => Pull();
        playerInput.actions["Interact"].performed += ctx => Interact();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float speed = playerInput.actions["Run"].IsPressed() ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(moveInput.x * speed, rb.velocity.y);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            Debug.Log($"Player {playerIndex + 1} Jumped!");
        }
    }

    private void Attack()
    {
        Debug.Log($"Player {playerIndex + 1} Attacked!");
    }

    private void Push()
    {
        Debug.Log($"Player {playerIndex + 1} is Pushing!");
    }

    private void Pull()
    {
        Debug.Log($"Player {playerIndex + 1} is Pulling!");
    }

    private void Interact()
    {
        Debug.Log($"Player {playerIndex + 1} Interacted!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
