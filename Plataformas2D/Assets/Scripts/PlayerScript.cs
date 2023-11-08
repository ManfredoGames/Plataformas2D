using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody2D rb;

    //movimiento

    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 32f;
    public bool isFacingRight = true;

    //coyotetime

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    //dash

    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 22f;
    public float dashTime = 0.15f;
    public float dashCooldown = 3f;

    //wallslide

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    //superjump
    public float superjumpPower = 60f;
    private bool isChargingJump = false;

    //groundcheck & wallcheck

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask cornerLayer;
    //camera

    [SerializeField] private GameObject _cameraFollow;
    private camerafollowObject _camerafollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    void Start()
    {
        _camerafollowObject = _cameraFollow.GetComponent<camerafollowObject>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //general if actions

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else if (coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        WallSlide();

        if (isDashing)
        {
            return;
        }

        if (!isFacingRight && horizontal > 0f)
        {
            Flip();
            if (IsWalled())
            {
                dashPower = -42f;
            }
            else
            {
                dashPower = 42f;
            }
            _camerafollowObject.CallTurn();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
            if (IsWalled())
            {
                dashPower = 42f;
            }
            else
            {
                dashPower = -42f;
            }
            _camerafollowObject.CallTurn();
        }


        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
 
    }

    //jump
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

            if (isWallSliding)
            {
                isWallSliding = false;
            }
        }
        else if (context.performed && isWallSliding)
        {
            rb.velocity = new Vector2(-transform.localScale.x * jumpingPower * 100f, jumpingPower);
            isWallSliding = false;
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.2f);
            coyoteTimeCounter = 0f;
        }
    }

    //superjump
    public void SuperJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isChargingJump = true;
        }
        else if (context.canceled)
        {
            isChargingJump = false;
        }

        if (context.performed && coyoteTimeCounter > 0f && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, superjumpPower);
            isChargingJump = false;
            Debug.Log("saltando");
        }
    }

    //dash
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(Dash());
        }

        if (isWallSliding)
        {
            isWallSliding = false;
        }
    }

    //move
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;

        if (isWallSliding)
        {
            isWallSliding = false;
        }
    }

    //grounded
    private bool IsGrounded()
    {
        float extraWidth = 0.3f; //tamano personaje / 2
        float groundCheckDistance = 1.2f;
        RaycastHit2D middleHit = Physics2D.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer | cornerLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position - new Vector3(extraWidth, 0, 0), -transform.up, groundCheckDistance, groundLayer | cornerLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position + new Vector3(extraWidth, 0, 0), -transform.up, groundCheckDistance, groundLayer | cornerLayer);

        Debug.DrawRay(transform.position, -transform.up * groundCheckDistance, Color.red);
        Debug.DrawRay(transform.position - new Vector3(extraWidth, 0, 0), -transform.up * groundCheckDistance, Color.red);
        Debug.DrawRay(transform.position + new Vector3(extraWidth, 0, 0), -transform.up * groundCheckDistance, Color.red);

        return middleHit.collider != null || leftHit.collider != null || rightHit.collider != null;
    }

    //walled
    private bool IsWalled()
    {
        float wallCheckDistance = 0.5f;
        Vector3 raycastOrigin = transform.position + new Vector3(0, 0.3f, 0);
        RaycastHit2D hitWall = Physics2D.Raycast(raycastOrigin, transform.right, wallCheckDistance, wallLayer | cornerLayer);

        Debug.DrawRay(raycastOrigin, transform.right * wallCheckDistance, Color.red);

        return hitWall.collider != null;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded())
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    private void FixedUpdate()
    {
        //dash
        if (isDashing || isChargingJump)
        {
            return;
        }

        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }
   
    //flip
    private void Flip()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
    }

    //dash
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