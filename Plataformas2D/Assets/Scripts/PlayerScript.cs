using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 32f;
    private bool isFacingRight = true;

    
    [SerializeField] private Rigidbody2D myRigidbody;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform cameraChanging;
    [SerializeField] private LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpingPower);

        }

        if (Input.GetButtonUp("Jump") && myRigidbody.velocity.y > 0f)
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, myRigidbody.velocity.y * 0.5f);

        }

    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void FixedUpdate()
    {
        myRigidbody.velocity = new Vector2(horizontal * speed, myRigidbody.velocity.y);
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
}
