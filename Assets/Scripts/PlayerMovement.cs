using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.1f;
    private float dashingCooldown = 0.5f;
    private bool doubleJump;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.5f;
    private float jumpBufferCounter;
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    private float accelerationTime = 5f / 50f;
    private float decelerationTime = 4f / 50f;
    private float targetSpeed;
    private float currentSpeed;
    private bool inVerticalTrigger = false;
    private float verticalAccelerationTime = 25f / 50f;
    private float targetVerticalSpeed = 5f;
    private float currentVerticalSpeed = 0f;
    private bool wasGroundedLastFrame = true;
    private bool wasFallingLastFrame = false;
    public ParticleSystem dust;
    private AudioSource audioSource;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float maxVerticalSpeed = 20f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    void Start(){
        audioSource = GetComponent<AudioSource>();
    }
    //void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();
    //    style.fontSize = 24; //
    //    style.normal.textColor = Color.white; //
    //
    //    GUI.Label(new Rect(10, 10, 300, 50), "Fall Speed: " + Mathf.Abs(rb.linearVelocity.y), style);
    //    GUI.Label(new Rect(10, 60, 300, 50), "wGLF: " + wasGroundedLastFrame, style);
    //}
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
        if (coyoteTimeCounter > 0f && !Input.GetKey(KeyCode.W))
        {
            doubleJump = false;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (jumpBufferCounter > 0)
        {
            if (coyoteTimeCounter > 0f || doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                dust.Play();
                jumpBufferCounter = 0f;
                doubleJump = !doubleJump;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.W) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        if (Input.GetKeyUp(KeyCode.Space) && canDash)
        {
            StartCoroutine(Dash());
        }

        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        bool isGrounded = IsGrounded();
        float fallSpeed = Mathf.Abs(rb.linearVelocityY);
        if (isGrounded && wasFallingLastFrame){
            if (fallSpeed > 17f){
                CameraFollow.Instance.ScreenShake(5f, 15f, 0.1f);
                audioSource.PlayOneShot(landSound);
                dust.Play();
            }
        }
        wasGroundedLastFrame = IsGrounded();
        wasFallingLastFrame = !isGrounded && rb.linearVelocity.y < 0;
        if (!isWallJumping)
        {
            if (horizontal != 0f)
            {
                targetSpeed = horizontal * speed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speed / accelerationTime * Time.fixedDeltaTime);
            }
            else
            {
                targetSpeed = 0f;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speed / decelerationTime * Time.fixedDeltaTime);
            }
            if (inVerticalTrigger)
            {
                currentVerticalSpeed = Mathf.MoveTowards(currentVerticalSpeed, targetVerticalSpeed, targetVerticalSpeed / verticalAccelerationTime * Time.fixedDeltaTime);
            }
            else
            {
                currentVerticalSpeed = 0f;
            }
            float newVerticalSpeed = rb.linearVelocity.y + currentVerticalSpeed;
            newVerticalSpeed = Mathf.Clamp(newVerticalSpeed, -maxVerticalSpeed, maxVerticalSpeed);
            rb.linearVelocity = new Vector2(currentSpeed, newVerticalSpeed);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("VerticalBoost"))
        {
            inVerticalTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("VerticalBoost"))
        {
            inVerticalTrigger = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.W) && wallJumpingCounter > 0)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Flip()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ((mousePosition.x > transform.position.x && !isFacingRight) ||
            (mousePosition.x < transform.position.x && isFacingRight))
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
        float originalSpeed = speed;
        speed = dashingPower;
        tr.emitting = true;
        CameraFollow.Instance.ScreenShake(10f, 2f, 0.2f);
        if (IsGrounded()){
            dust.Play();
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        else if (!IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 16f);
        }

        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        speed = originalSpeed;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
