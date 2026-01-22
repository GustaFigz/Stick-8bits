using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Slopes")]
    [SerializeField] private float slopeRayLength = 0.6f;
    [SerializeField] private float maxSlopeAngle = 50f;

    [Header("Animator Params")]
    [SerializeField] private string isRunningParameter = "IsRunning";
    [SerializeField] private string attackTriggerParameter = "Attack";

    [Header("Attack")]
    [SerializeField] private float attackMovementMultiplier = 0.5f; // Permite movimento reduzido durante ataque

    private int _isRunningHash;
    private int _attackHash;

    private float _horizontalInput;
    private bool _jumpRequested;
    private bool _isFacingRight = true;

    private bool _isAttacking;
    private Coroutine _attackRoutine;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

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
        bool grounded = IsGrounded();
        float currentSpeed = _isAttacking ? speed * attackMovementMultiplier : speed;

        // Jump
        if (_jumpRequested && grounded)
        {
            Vector2 vJump = rb.linearVelocity;
            vJump.x = _horizontalInput * currentSpeed;
            vJump.y = jumpForce;
            rb.linearVelocity = vJump;
            _jumpRequested = false;
            return;
        }

        _jumpRequested = false;

        // Movimento
        if (grounded)
        {
            int mask = groundLayer.value == 0 ? Physics2D.DefaultRaycastLayers : groundLayer.value;
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, slopeRayLength, mask);

            if (hit.collider != null)
            {
                Vector2 n = hit.normal;
                float angle = Vector2.Angle(n, Vector2.up);

                if (angle > 0.01f && angle <= maxSlopeAngle && !Mathf.Approximately(_horizontalInput, 0f))
                {
                    Vector2 t = Vector2.Perpendicular(n).normalized;
                    Vector2 desired = new Vector2(_horizontalInput, 0f);
                    if (Vector2.Dot(t, desired) < 0f) t = -t;

                    rb.linearVelocity = t * (Mathf.Abs(_horizontalInput) * currentSpeed);
                    return;
                }
            }

            Vector2 v = rb.linearVelocity;
            v.x = _horizontalInput * currentSpeed;
            rb.linearVelocity = v;
        }
        else
        {
            Vector2 v = rb.linearVelocity;
            v.x = _horizontalInput * currentSpeed;
            rb.linearVelocity = v;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed && !context.canceled)
            return;

        float input = 0f;

        if (context.valueType == typeof(Vector2))
            input = context.ReadValue<Vector2>().x;
        else
            input = context.ReadValue<float>();

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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (animator == null) return;

        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);

        _isAttacking = true;
        animator.SetTrigger(_attackHash);

        _attackRoutine = StartCoroutine(AttackLockRoutine());
    }

    private IEnumerator AttackLockRoutine()
    {
        yield return new WaitForSeconds(0.5f); // Duração da flag de ataque
        _isAttacking = false;
        _attackRoutine = null;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        int mask = groundLayer.value == 0 ? Physics2D.DefaultRaycastLayers : groundLayer.value;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);
    }

    private void FlipIfNeeded()
    {
        if (_horizontalInput > 0f && !_isFacingRight)
            Flip();
        else if (_horizontalInput < 0f && _isFacingRight)
            Flip();
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
            return;

        bool isMoving = !Mathf.Approximately(_horizontalInput, 0f);
        animator.SetBool(_isRunningHash, isMoving);
    }

    private void CacheAnimatorHashes()
    {
        _isRunningHash = Animator.StringToHash(isRunningParameter ?? string.Empty);
        _attackHash = Animator.StringToHash(attackTriggerParameter ?? string.Empty);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * slopeRayLength);
    }
}
