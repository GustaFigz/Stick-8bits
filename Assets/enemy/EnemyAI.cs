using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackLockTime = 0.35f; // tempo que fica "parado" a atacar

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isRunningParameter = "IsRunning";
    [SerializeField] private string attackTriggerParameter = "Attack";

    private Rigidbody2D _rb;
    private float _nextAttackTime;
    private bool _isAttacking;

    private int _isRunningHash;
    private int _attackHash;
    private Coroutine _attackRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _isRunningHash = Animator.StringToHash(isRunningParameter ?? string.Empty);
        _attackHash = Animator.StringToHash(attackTriggerParameter ?? string.Empty);
    }

    private void Start()
    {
        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float dist = Vector2.Distance(_rb.position, player.position);

        if (_isAttacking)
        {
            // durante ataque: não empurra, mas deixa gravidade funcionar
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        // Chase: só controla X, deixa Y para a gravidade
        Vector2 v = _rb.linearVelocity;

        if (dist > stopDistance)
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            v.x = dir * moveSpeed;
        }
        else
        {
            v.x = 0f;
        }

        _rb.linearVelocity = v;

        // Animator: correr se estiver a mover no X
        if (animator != null)
            animator.SetBool(_isRunningHash, Mathf.Abs(v.x) > 0.01f);

        // Ataque
        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            StartAttack();
            _nextAttackTime = Time.time + attackCooldown;
        }

        FlipTowardsPlayer();
    }

    private void StartAttack()
    {
        _isAttacking = true;

        if (animator != null)
        {
            animator.SetBool(_isRunningHash, false);
            animator.SetTrigger(_attackHash); // Trigger para entrar no state attack [web:173]
        }

        TryDamagePlayer(); // simples (dano instantâneo). Depois pode virar Animation Event.

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
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }

    private void FlipTowardsPlayer()
    {
        float dir = player.position.x - transform.position.x;
        if (Mathf.Abs(dir) < 0.001f) return;

        Vector3 s = transform.localScale;
        s.x = Mathf.Sign(dir) * Mathf.Abs(s.x);
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
