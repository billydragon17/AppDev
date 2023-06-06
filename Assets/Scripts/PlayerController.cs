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
    [SerializeField] private float attackTime = 0.7f;
    [SerializeField] private float attackCooldown = 0.5f;
    public float attackRange = 0.5f;

    //Ints
    [SerializeField] private float attackDamage = 40;
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;
    //public int damageTaken;

    //  Bools
    private bool canDoubleJump;
    private bool canDash = true;
    private bool isDashing;
    private bool isFacingRight;
    private bool canAttack = true;
    private bool isAttacking;
    private bool isDoubleJumping;

    //  Movemement state
    private enum MovementState {idle, running, jumping, falling, doubleJumping, dashing, attacking, airAttacking};

    //  Unity References
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private BoxCollider2D coll;
    public Transform attackPoint;
    
    public LayerMask enemyLayers;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private TrailRenderer tr;

    //  Other References
    [SerializeField] private HealthBar healthBar;
    //public EnemyController enemy;

    public float CurrentDmg;


    /*  Update methods  */

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (isDashing)
        {
            return;
        }

        if (isAttacking && IsGrounded())
        {
            return;
        }
        //  Movements   
        Jump();
        Moving();
        StartCoroutine(Dash());

        //  Attacks
        StartCoroutine(NormalAttack());

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
            isDoubleJumping = false;
            canDoubleJump = false;
        }
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }
    
    
    /*  Movement Methods  */
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            canDoubleJump = true;
        }
            //  Double Jump
        else if (Input.GetButtonDown("Jump") && canDoubleJump)
        {
            rb.velocity = Vector2.up * doubleJumpForce;
            isDoubleJumping = true;
            canDoubleJump = false;
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
    
    /*  Attack Methods  */
    private IEnumerator NormalAttack()
    {
        if (Input.GetKeyDown(KeyCode.J) && canAttack)
        {
            canAttack = false;
            isAttacking = true;
            //Debug.Log(isAttacking);
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyController>().TakeDamage(attackDamage);     // COMMAND PATTERN
            }

            yield return new WaitForSeconds(attackTime);
            isAttacking = false;
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }
    }

    void OnDrawGizmosSelected()     // show atk range
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    
    /*  Update Animation State  */
    private void UpdateAnimationState()
    {
        MovementState state;
        //      change to switch cases      //
        //  running
        switch (moveInput)
        {
            case 0f:
                state = MovementState.idle;
                break;
            default:
                state = MovementState.running;
                break;
        }


        if (isAttacking == true && IsGrounded())
        {
            state = MovementState.attacking;
        }
        else if (isAttacking == true && !IsGrounded())
        {
            state = MovementState.airAttacking;
        }

        // dashing
        if (isDashing == true)
        {
            state = MovementState.dashing; 
        }

        // jumping & falling
        if (rb.velocity.y > 0.1f && canDoubleJump == true)      // jumping
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y > 0.1f && isDoubleJumping == true)       // double-jumping
        {
            state = MovementState.doubleJumping;
        }

        if (rb.velocity.y < -0.1f)
        {
            // if atk button pressed, change to down atk anim
            state = MovementState.falling;
        }

        Debug.Log("Current state " + state);

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

    private void TakeDamage(float amount)     // damage when touch enemy
    {
        currentHealth -= amount;

        healthBar.SetHealth(currentHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            CurrentDmg = collision.gameObject.GetComponent<EnemyController>().damage;
            TakeDamage(CurrentDmg);
            Debug.Log("Player took damage");
        }
        
    }
}
