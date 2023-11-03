using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 32f;
    private bool isFacingRight = true;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private bool canDash = true;
    private bool isDashing;
    public float dashPower = 40f; //revisar (creo que no va)
    public float dashTime = 0.15f; //revisar (creo que no va)
    public float dashCooldown = 3f; //revisar (creo que no va)

    
    private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
  

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (isDashing)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

            
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.2f);
            coyoteTimeCounter = 0f;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());

        }

        Flip();

    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashPower * 0.45f, 0f);
        yield return new WaitForSeconds(dashTime * 0.45f);
        rb.velocity = new Vector2(transform.localScale.x * dashPower * 0.35f, 0f);
        yield return new WaitForSeconds(dashTime * 0.35f);
        rb.velocity = new Vector2(transform.localScale.x * dashPower * 0.25f, 0f);
        yield return new WaitForSeconds(dashTime * 0.25f);
        rb.velocity = new Vector2(transform.localScale.x * dashPower * 0.15f, 0f);
        yield return new WaitForSeconds(dashTime * 0.15f);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
