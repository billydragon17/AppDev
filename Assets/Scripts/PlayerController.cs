using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //  Floats
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float doubleJumpForce;
    private float moveInput;
    public float dashingPower = 10f;   
    public float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    //  Bools
    private bool doubleJump;
    private bool canDash = true;
    private bool isDashing;
    private bool isFacingRight;
    

    //  Movemement state
    private enum MovementState {idle, running, jumping, falling, doubleJumping, dashing};

    //  Unity References
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private BoxCollider2D coll;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private TrailRenderer tr;

    /*  Update methods  */

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing)
        {
            return;
        }
        
        //  Movements   
        Jump();
        Moving();
        StartCoroutine(Dash());
        
        //  Animation
        UpdateAnimationState();
        
        //  others
        Flip();
    }

    //  Check for ground
    private bool IsGrounded()
    {
        //  Preventing double jump while on ground
        if (!Input.GetButtonDown("Jump"))
        {
            doubleJump = false;
        }
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }
    
    
    /*  Movement Methods    */
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            doubleJump = true;
            anim.SetInteger("State", 2);
        }
            //  Double Jump
        else if (Input.GetButtonDown("Jump") && doubleJump)
        {
            rb.velocity = Vector2.up * doubleJumpForce;
            doubleJump = false;
        }
    }

    void Moving()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private IEnumerator Dash()
    {
        if (Input.GetKeyDown(KeyCode.O) && canDash)
        {
            canDash = false;        //prevents dashing while already dashing
            isDashing = true;
            float originalGravity = rb.gravityScale;        // turning off gravity while dashing
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);       // the dash
            tr.emitting = true;     // turn on trail
            yield return new WaitForSeconds(dashingTime);       // stop dashing after <dashingTime>
            tr.emitting = false;    // turn off trail
            rb.gravityScale = originalGravity;      // turn gravity back on
            isDashing = false;      
            yield return new WaitForSeconds(dashingCooldown);       // time until can dash again
            canDash = true;
        }
    }
    
    
    /*  Update Animation State  */
    private void UpdateAnimationState()
    {
        MovementState state;

        //  running
        if (moveInput != 0f)
        {
            state = MovementState.running;
        }
        else
        {
            state = MovementState.idle;
        }

        // dashing
        if (isDashing == true)
        {
            state = MovementState.dashing;
        }

        // jumping & falling
        if (rb.velocity.y > 0.1f && doubleJump == true)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y > 0.1f && doubleJump == false)
        {
            state = MovementState.doubleJumping;
        }
        if (rb.velocity.y < -0.1f)
        {
            state = MovementState.falling;
        }
        anim.SetInteger("State", (int) state);
    }

    /*  Other Methods   */
    private void Flip()
    {
        if (isFacingRight && moveInput > 0f || !isFacingRight && moveInput < 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
