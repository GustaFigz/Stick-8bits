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
    [SerializeField] private float maxSlopeAngle = 50f; // 45° + folga

    [Header("Animator Params")]
    [SerializeField] private string isRunningParameter = "IsRunning";
    [SerializeField] private string attackTriggerParameter = "Attack";

    [Header("Attack")]
    [SerializeField] private float attackLockTime = 0.35f;

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

        if (!_isAttacking)
            UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (_isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        bool grounded = IsGrounded();

        // Jump (prioridade)
        if (_jumpRequested && grounded)
        {
            Vector2 vJump = rb.linearVelocity;
            vJump.x = _horizontalInput * speed;
            vJump.y = jumpForce;
            rb.linearVelocity = vJump;
            _jumpRequested = false;
            return;
        }

        _jumpRequested = false;

        // Movimento
        if (grounded)
        {
            // Raycast para pegar a normal do chão (para slope)
            int mask = groundLayer.value == 0 ? Physics2D.DefaultRaycastLayers : groundLayer.value;
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, slopeRayLength, mask);

            if (hit.collider != null)
            {
                Vector2 n = hit.normal; // normal da superfície [web:88]
                float angle = Vector2.Angle(n, Vector2.up);

                // Se está numa rampa "andável" e existe input
                if (angle > 0.01f && angle <= maxSlopeAngle && !Mathf.Approximately(_horizontalInput, 0f))
                {
                    // Tangente paralela ao chão
                    Vector2 t = Vector2.Perpendicular(n).normalized;

                    // Faz a tangente apontar para o lado do input
                    Vector2 desired = new Vector2(_horizontalInput, 0f);
                    if (Vector2.Dot(t, desired) < 0f) t = -t;

                    rb.linearVelocity = t * (Mathf.Abs(_horizontalInput) * speed);
                    return;
                }
            }

            // Chão plano (ou sem input / rampa inválida): comportamento original
            Vector2 v = rb.linearVelocity;
            v.x = _horizontalInput * speed;
            rb.linearVelocity = v;
        }
        else
        {
            // No ar: controla só X, mantém Y
            Vector2 v = rb.linearVelocity;
            v.x = _horizontalInput * speed;
            rb.linearVelocity = v;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (_isAttacking) return;

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
        if (_isAttacking) return;

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
        _horizontalInput = 0f;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        animator.SetBool(_isRunningHash, false);
        animator.SetTrigger(_attackHash);

        _attackRoutine = StartCoroutine(AttackLockRoutine());
    }

    private IEnumerator AttackLockRoutine()
    {
        yield return new WaitForSeconds(attackLockTime);
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

        // visual do raycast de slope
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * slopeRayLength);
    }
}
