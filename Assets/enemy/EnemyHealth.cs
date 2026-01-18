using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    public int CurrentHealth { get; private set; }

    [Header("Death")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deathTriggerParameter = "Death";
    [SerializeField] private float deathDestroyDelay = 0.8f;

    private bool _isDead;
    private int _deathHash;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _deathHash = Animator.StringToHash(deathTriggerParameter ?? string.Empty);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || _isDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (CurrentHealth == 0)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (animator != null)
            animator.SetTrigger(_deathHash);

        var ai = GetComponent<EnemyAI>();
        if (ai != null)
            ai.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        var colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }
}
