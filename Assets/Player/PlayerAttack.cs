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

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] [Range(0f, 1f)] private float attackVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float hitVolume = 0.8f;

    private float _nextAttackTime;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && (attackSound != null || hitSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

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
        
        PlayAttackSound();
        DealDamageNow();
    }

    // Podes também chamar isto por Animation Event (opcional)
    public void DealDamageNow()
    {
        Debug.Log($"[ATTACK] Attempting attack at {attackPoint.position}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        Debug.Log($"[ATTACK] Found {hits.Length} colliders in range");

        bool hitEnemy = false;

        foreach (var hit in hits)
        {
            Debug.Log($"[ATTACK] Hit object: {hit.name}");

            var health = hit.GetComponentInParent<EnemyHealth>();

            if (health != null)
            {
                Debug.Log($"[ATTACK] Dealing {damage} damage to {hit.name}");
                health.TakeDamage(damage);
                hitEnemy = true;
            }
            else
            {
                Debug.LogWarning($"[ATTACK] {hit.name} has no EnemyHealth component!");
            }
        }

        if (hitEnemy)
            PlayHitSound();
    }

    private void PlayAttackSound()
    {
        if (attackSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(attackSound, attackVolume);
        }
    }

    private void PlayHitSound()
    {
        if (hitSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(hitSound, hitVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
