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

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] [Range(0f, 1f)] private float hurtVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float deathVolume = 1f;

    private bool _isDead;
    private int _deathHash;
    private AudioSource _audioSource;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _deathHash = Animator.StringToHash(deathTriggerParameter ?? string.Empty);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && (hurtSound != null || deathSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || _isDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (CurrentHealth == 0)
            Die();
        else
            PlayHurtSound();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        PlayDeathSound();

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

    private void PlayHurtSound()
    {
        if (hurtSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(hurtSound, hurtVolume);
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(deathSound, deathVolume);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }
}
