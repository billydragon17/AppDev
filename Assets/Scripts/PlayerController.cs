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
    private BoxCollider2D coll;

    [SerializeField] private LayerMask jumpableGround;

    //private bool isGrounded;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //  Movements   
        Jump();
        Moving();
        
        //  Animation
        UpdateAnimationState();
        

    }

    //  Check for ground
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }
    
    
    //  Movement Methods
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            
        }

        //  Double Jump
        //if (Input.GetButtonDown("Jump") && )
    }

    void Moving()
    {
         moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
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
