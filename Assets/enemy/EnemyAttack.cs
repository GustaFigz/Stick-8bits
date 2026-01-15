using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseAttack : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.6f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.0f;

    [Header("Target")]
    [SerializeField] private Transform player; // podes arrastar pelo Inspector

    private Rigidbody2D _rb;
    private float _nextAttackTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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

        // 1) Perseguir até uma distância mínima
        if (dist > stopDistance)
        {
            Vector2 nextPos = Vector2.MoveTowards(
                _rb.position,
                player.position,
                moveSpeed * Time.fixedDeltaTime
            ); // MoveTowards move o ponto atual na direção do alvo sem “passar” do max delta [web:102]

            _rb.MovePosition(nextPos); // MovePosition move o Rigidbody2D para uma posição via física [web:94]
        }

        // 2) Atacar (dar dano) quando estiver em range + cooldown
        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            TryDamagePlayer();
            _nextAttackTime = Time.time + attackCooldown;
        }

        // (Opcional) virar para o player
        FlipTowardsPlayer();
    }

    private void TryDamagePlayer()
    {
        // assume que o teu player tem o script PlayerHealth
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
