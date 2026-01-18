using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private float enableChaseDelay = 0.25f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackLockTime = 0.35f;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fallingThreshold = -0.1f; // yVel < isso = a cair

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isRunningParameter = "IsRunning";
    [SerializeField] private string attackTriggerParameter = "Attack";
    [SerializeField] private string isFallingParameter = "IsFalling";

    private Rigidbody2D _rb;

    private bool _isAttacking;
    private bool _canChase;
    private float _nextAttackTime;

    private int _isRunningHash;
    private int _attackHash;
    private int _isFallingHash;

    private Coroutine _attackRoutine;
    private Coroutine _enableChaseRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _isRunningHash = Animator.StringToHash(isRunningParameter ?? string.Empty);
        _attackHash = Animator.StringToHash(attackTriggerParameter ?? string.Empty);
        _isFallingHash = Animator.StringToHash(isFallingParameter ?? string.Empty);

        // fallback: se não arrastares um groundCheck, usa o próprio inimigo
        if (groundCheck == null) groundCheck = transform;
    }

    private void OnEnable()
    {
        _isAttacking = false;
        _canChase = false;
        _nextAttackTime = 0f;

        if (_enableChaseRoutine != null) StopCoroutine(_enableChaseRoutine);
        _enableChaseRoutine = StartCoroutine(EnableChaseAfterDelay());
    }

    private IEnumerator EnableChaseAfterDelay()
    {
        yield return new WaitForSeconds(enableChaseDelay);
        _canChase = true;
        _enableChaseRoutine = null;
    }

    private void Start()
    {
        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag(playerTag);
            if (pObj != null) player = pObj.transform;
        }
    }

    private void Update()
    {
        if (animator == null) return;

        bool grounded = IsGrounded();
        // linearVelocity é a velocidade linear do Rigidbody2D. [page:1]
        bool isFalling = !grounded && _rb.linearVelocity.y < fallingThreshold;

        animator.SetBool(_isFallingHash, isFalling);
    }

    private void FixedUpdate()
    {
        if (player == null || !_canChase) return;

        bool grounded = IsGrounded();

        Vector2 pos = _rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        float distSqr = toPlayer.sqrMagnitude;

        float stopSqr = stopDistance * stopDistance;
        float attackSqr = attackRange * attackRange;

        if (_isAttacking)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        // Movimento (só X) — se estiver no ar, não controla X (fica mais natural para “drop” do avião)
        Vector2 v = _rb.linearVelocity;

        if (grounded)
        {
            if (distSqr > stopSqr)
            {
                float dir = Mathf.Sign(toPlayer.x);
                v.x = dir * moveSpeed;
            }
            else
            {
                v.x = 0f;
            }
        }
        else
        {
            v.x = 0f;
        }

        _rb.linearVelocity = v;

        if (animator != null)
            animator.SetBool(_isRunningHash, grounded && Mathf.Abs(v.x) > 0.01f);

        // Ataque só quando está no chão (evita atacar “a cair”)
        if (grounded && distSqr <= attackSqr && Time.time >= _nextAttackTime)
        {
            StartAttack();
            _nextAttackTime = Time.time + attackCooldown;
        }

        if (grounded) FlipTowards(toPlayer.x);
    }

    private bool IsGrounded()
    {
        // OverlapCircle retorna um Collider2D se encontrar algo dentro do círculo e aceita LayerMask. [web:73]
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
    }

    private void StartAttack()
    {
        _isAttacking = true;

        if (animator != null)
        {
            animator.SetBool(_isRunningHash, false);
            animator.SetTrigger(_attackHash);
        }

        TryDamagePlayer();

        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _attackRoutine = StartCoroutine(AttackLockRoutine());
    }

    private IEnumerator AttackLockRoutine()
    {
        yield return new WaitForSeconds(attackLockTime);
        _isAttacking = false;
        _attackRoutine = null;
    }

    private void TryDamagePlayer()
    {
        if (player == null) return;

        float dist = Vector2.Distance(_rb.position, player.position);
        if (dist > attackRange) return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);
    }

    private void FlipTowards(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.001f) return;
        Vector3 s = transform.localScale;
        s.x = Mathf.Sign(xDir) * Mathf.Abs(s.x);
        transform.localScale = s;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
