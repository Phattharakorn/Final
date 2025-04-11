using UnityEngine;

public class P2SpriteFlipController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Assign in Inspector or auto-find

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            spriteRenderer.flipX = true; // Flip left
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            spriteRenderer.flipX = false; // Face right
        }
    }
}
