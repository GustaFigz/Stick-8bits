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

    [Header("Animator Params")]
    [SerializeField] private string isRunningParameter = "IsRunning"; // ajusta ao teu Animator
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

        // Enquanto ataca, não deixa o bool de corrida “forçar” run por cima do attack
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
        // opcional: bloquear input durante ataque
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

    // Ligar no PlayerInput -> Events -> Attack -> performed
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (animator == null) return;

        // Reinicia o lock se clicar de novo (opcional)
        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);

        _isAttacking = true;
        _horizontalInput = 0f;

        // Para movimento horizontal imediatamente
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Evita que "run" fique true no mesmo frame
        animator.SetBool(_isRunningHash, false);

        // Dispara o Trigger do ataque
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
    }
}
