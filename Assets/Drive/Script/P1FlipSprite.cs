using UnityEngine;

public class P1SpriteFlipController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Assign in Inspector or auto-find

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.flipX = true; // Flip left
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.flipX = false; // Face right
        }
    }
}
