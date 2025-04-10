using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SmoothFall2D : MonoBehaviour
{
    [Header("Fall Settings")]
    public float gravityScale = 1f;
    public float maxFallSpeed = -10f;
    public float fallSmoothing = 2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    void FixedUpdate()
    {
        // Smooth fall
        if (rb.velocity.y < 0)
        {
            float smoothFall = Mathf.Lerp(rb.velocity.y, maxFallSpeed, fallSmoothing * Time.fixedDeltaTime);
            rb.velocity = new Vector2(rb.velocity.x, smoothFall);
        }
    }
}
