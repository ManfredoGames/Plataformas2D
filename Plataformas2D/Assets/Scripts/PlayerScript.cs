using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 32f;
    public bool isFacingRight = true;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 22f; 
    public float dashTime = 0.15f; 
    public float dashCooldown = 3f;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    
    private Rigidbody2D rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private GameObject _cameraFollow;



    private camerafollowObject _camerafollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    // Start is called before the first frame update
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
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        WallSlide();

        if (isDashing)
        {
            return;
        }

        //rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if(!isFacingRight && horizontal > 0f)
        {
            Flip();
            dashPower = 42f;
            _camerafollowObject.CallTurn();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
            dashPower = -42f;
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
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);


        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.2f);
            coyoteTimeCounter = 0f;
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(Dash());
            
        }
        
    }

    public void Move(InputAction.CallbackContext context)
    {

        horizontal = context.ReadValue<Vector2>().x;
    }


    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
   
    private void WallSlide()
    {
        if (IsWalled() &&  !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
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
