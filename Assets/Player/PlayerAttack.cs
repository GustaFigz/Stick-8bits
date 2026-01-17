using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Hit")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.3f;

    private float _nextAttackTime;


    private void Start()
    {
        Debug.Log("PlayerAttack START OK");
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"PlayerAttack OnAttack: {context.phase}");
        if (!context.performed) return;

        if (attackPoint == null) return;
        if (Time.time < _nextAttackTime) return;

        _nextAttackTime = Time.time + attackCooldown;
        DealDamageNow();
    }


    // Podes também chamar isto por Animation Event (opcional)
    public void DealDamageNow()
    {
        Debug.Log($"[ATTACK] Attempting attack at {attackPoint.position}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        Debug.Log($"[ATTACK] Found {hits.Length} colliders in range");

        foreach (var hit in hits)
        {
            Debug.Log($"[ATTACK] Hit object: {hit.name}");

            var health = hit.GetComponentInParent<EnemyHealth>();

            if (health != null)
            {
                Debug.Log($"[ATTACK] Dealing {damage} damage to {hit.name}");
                health.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"[ATTACK] {hit.name} has no EnemyHealth component!");
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
