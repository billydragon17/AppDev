using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //  Floats
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private float moveInput;

    //  Movemement state
    private enum MovementState {idle, running, jumping, falling};

    //  Unity References
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private bool isGrounded;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        //  Movements   
        Jump();
        
        //  Animation
        UpdateAnimationState();
        

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    //  Movement Methods
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
        }
    }
    
    
    //  Update Animation State
    private void UpdateAnimationState()
    {
        MovementState state;

        //  running
        if (moveInput > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (moveInput < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        //  jumping & falling
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.falling;
        }
        anim.SetInteger("State", (int) state);
    }

}
