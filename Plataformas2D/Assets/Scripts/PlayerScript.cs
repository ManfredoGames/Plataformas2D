using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody2D rb;

    public struct InputActionTime
    {
        public InputAction.CallbackContext context;
        public float time;
        public string action;

        public InputActionTime(InputAction.CallbackContext context, float time, string action)
        {
            this.context = context;
            this.time = time;
            this.action = action;
        }
    }

    private List<InputActionTime> inputBuffer = new List<InputActionTime>();
    private float bufferTime = 0.6f;

    //movimiento

    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 32f;
    public bool isFacingRight = true;
    private float maxvelocity = -45f;
    public float fallMultiplier = 0.2f;
    private bool isJumping;
    private float gravityFalling;

    //coyotetime

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float wallJumpCoyoteTime = 0.2f;
    private float wallJumpCoyoteTimeCounter;

    //dash

    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 30f;
    public float dashTime = 0.15f;
    public float dashCooldown = 3f;

    //wallslide

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool canControl = true;

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
        gravityFalling = rb.gravityScale;
        _camerafollowObject = _cameraFollow.GetComponent<camerafollowObject>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void BufferJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputBuffer.Add(new InputActionTime(context, Time.time, "Jump"));
        }
    }

    public void BufferDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            inputBuffer.Add(new InputActionTime(context, Time.time, "Dash"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        //general if actions

        for (int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (Time.time - inputBuffer[i].time <= bufferTime)
            {
                switch (inputBuffer[i].action)
                {
                    case "Jump":
                        Jump(inputBuffer[i].context);
                        break;
                    case "Dash":
                        Dash(inputBuffer[i].context);
                        break;
                }
                inputBuffer.RemoveAt(i);
            }
            else
            {
                break;
            }
        }

        if (coyoteTimeCounter < 0f || wallJumpCoyoteTimeCounter < 0f)
        {
            isJumping = false;
        }

        if (rb.velocity.y < maxvelocity)
        { 
            rb.velocity = new Vector2(rb.velocity.x, maxvelocity);
        }

        if (!isWallSliding)
        {
            wallJumpCoyoteTimeCounter -= Time.deltaTime;
        }

        if (isWallSliding)
        {
            wallJumpCoyoteTimeCounter = wallJumpCoyoteTime;
        }

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

        if (horizontal > 0f && !isFacingRight)
        {
            Flip();
            _camerafollowObject.CallTurn();
        }
        else if (horizontal < 0f && isFacingRight)
        {
            Flip();
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
    private IEnumerator WallJumpCoroutine()
    {
        float jumpTime = 0.1f;
        float timeElapsed = 0;

        float direction = isFacingRight ? -1 : 1;

        canControl = false;

        while (timeElapsed < jumpTime)
        {
            rb.velocity = new Vector2(12.0f * direction, 12.0f);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        canControl = true;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (coyoteTimeCounter > 0f || (isWallSliding && wallJumpCoyoteTimeCounter > 0f))
            {
                isJumping = true;

                if (wallJumpCoyoteTimeCounter > 0f)
                {
                    StartCoroutine(WallJumpCoroutine());
                    isWallSliding = false;
                }
                else
                {
                    rb.AddForce(new Vector2(0, jumpingPower), ForceMode2D.Impulse);
                }
            }
        }
    }


    //superjump
    public void SuperJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isChargingJump = true;
            rb.velocity = Vector2.zero;
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
    }

    //grounded
    private bool IsGrounded()
    {
        float extraWidth = 0.2f; //tamano personaje / 2
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
        Vector3 raycastOrigin = transform.position + new Vector3(0, 0.15f, 0);
        RaycastHit2D hitWall = Physics2D.Raycast(raycastOrigin, transform.right, wallCheckDistance, wallLayer | cornerLayer);

        Debug.DrawRay(raycastOrigin, transform.right * wallCheckDistance, Color.red);

        return hitWall.collider != null;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded())
        {
            if ((isFacingRight && horizontal > 0) || (!isFacingRight && horizontal < 0) || isWallSliding)
            {
                isWallSliding = true;
            }
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
        if (isDashing || isChargingJump || !canControl)
        {
            return;
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity -= new Vector2(rb.velocity.x, rb.gravityScale * -2f * Time.fixedDeltaTime);
        }
        else
        {
            rb.gravityScale = gravityFalling;

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
    private IEnumerator Dash1()
    {
        isJumping = false;
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

    private IEnumerator Dash()
    {
        isJumping = false;
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float elapsedTime = 0;
        float dashDuration = dashTime * 1.5f;

        float direction = isFacingRight ? 1 : -1;
        if (IsWalled())
        {
            direction *= -1;
            Flip();
        }

        while (elapsedTime < dashDuration)
        {
            float t = elapsedTime / dashDuration;
            float currentDashSpeed = Mathf.Lerp(dashPower, 0, t);
            rb.velocity = new Vector2(direction * currentDashSpeed, 0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


}