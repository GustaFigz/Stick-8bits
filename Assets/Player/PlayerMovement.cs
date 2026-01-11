using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    public float speed = 8f;

    public float jumpForce = 16f;

    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string isRunningParameter = "isRunning";

    private int _isRunningHash;

    private float _horizontalInput;
    private bool _jumpRequested;
    private bool _isFacingRight = true;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        CacheAnimatorHashes();
    }

    private void OnValidate()
    {
        CacheAnimatorHashes();
    }

    private void Update()
    {
        FlipIfNeeded();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = _horizontalInput * speed;

        if (_jumpRequested && IsGrounded())
        {
            velocity.y = jumpForce;
            _jumpRequested = false;
        }

        rb.linearVelocity = velocity;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed && !context.canceled)
        {
            return;
        }

        float input = 0f;
        var valueType = context.valueType;

        if (valueType == typeof(Vector2))
        {
            input = context.ReadValue<Vector2>().x;
        }
        else
        {
            input = context.ReadValue<float>();
        }

        _horizontalInput = context.canceled ? 0f : input;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpRequested = true;
        }
        else if (context.canceled && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            return false;
        }

        int mask = groundLayer.value == 0 ? Physics2D.DefaultRaycastLayers : groundLayer.value;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);
    }

    private void FlipIfNeeded()
    {
        if (_horizontalInput > 0f && !_isFacingRight)
        {
            Flip();
        }
        else if (_horizontalInput < 0f && _isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void UpdateAnimator()
    {
        if (animator == null || string.IsNullOrEmpty(isRunningParameter))
        {
            return;
        }

        bool isMoving = !Mathf.Approximately(_horizontalInput, 0f);
        animator.SetBool(_isRunningHash, isMoving);
    }

    private void CacheAnimatorHashes()
    {
        _isRunningHash = Animator.StringToHash(isRunningParameter ?? string.Empty);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
