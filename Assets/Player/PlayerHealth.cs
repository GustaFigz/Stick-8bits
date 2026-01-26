using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    [Header("Damage")]
    [Tooltip("Tempo de invencibilidade ap\u00f3s tomar dano. Evita perder todas as vidas no mesmo frame por m\u00faltiplos colliders.")]
    [SerializeField] private float invincibilitySeconds = 0.75f;

    public int MaxLives => maxLives;
    public int CurrentLives { get; private set; }
    public bool IsDead => CurrentLives <= 0;

    public event System.Action<int> OnLivesChanged;

    private float _nextTimeCanTakeDamage;

    private void Awake()
    {
        CurrentLives = maxLives;
    }

    private void OnEnable()
    {
        // Garante que a UI receba o valor atual mesmo se ela se inscrever depois.
        OnLivesChanged?.Invoke(CurrentLives);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        // Evita tomar v\u00e1rios danos imediatamente (ex: inimigo com mais de um collider/trigger).
        if (Time.time < _nextTimeCanTakeDamage) return;
        _nextTimeCanTakeDamage = Time.time + invincibilitySeconds;

        // Dano = perde 1 vida (cora\u00e7\u00e3o)
        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        OnLivesChanged?.Invoke(CurrentLives);

        if (CurrentLives == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        // Cura = ganha 1 vida (cora\u00e7\u00e3o)
        CurrentLives = Mathf.Min(maxLives, CurrentLives + 1);
        OnLivesChanged?.Invoke(CurrentLives);
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}
