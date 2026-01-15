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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // usar performed para disparar 1x por clique [web:140]
        if (attackPoint == null) return;
        if (Time.time < _nextAttackTime) return;

        _nextAttackTime = Time.time + attackCooldown;
        DealDamageNow();
    }

    // Podes também chamar isto por Animation Event (opcional)
    public void DealDamageNow()
    {
        // OverlapCircleAll devolve todos os colliders no círculo (bom para melee) [web:134]
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (var hit in hits)
        {
            // GetComponentInParent para funcionar mesmo se o collider estiver num filho
            var health = hit.GetComponentInParent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
