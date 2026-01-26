using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Arraste aqui os 3 GameObjects de cora\u00e7\u00e3o (imagem) na ordem: 1, 2, 3")]
    [SerializeField] private GameObject[] hearts;

    private void Awake()
    {
        if (playerHealth == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged += HandleLivesChanged;
    }

    private void Start()
    {
        if (playerHealth != null)
            HandleLivesChanged(playerHealth.CurrentLives);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged -= HandleLivesChanged;
    }

    private void HandleLivesChanged(int currentLives)
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < currentLives);
        }
    }
}
