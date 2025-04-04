using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EZMoveFortest : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moveInput;
    public float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }
}
