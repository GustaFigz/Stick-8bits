using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private PlayerCoinCollector playerCoinCollector;

    [Header("Format")]
    [SerializeField] private string textFormat = "Moedas: {0}";

    private void Start()
    {
        if (playerCoinCollector == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerCoinCollector = player.GetComponent<PlayerCoinCollector>();
        }

        if (playerCoinCollector != null)
        {
            playerCoinCollector.OnCoinsChanged.AddListener(UpdateCoinDisplay);
            UpdateCoinDisplay(playerCoinCollector.CurrentCoins);
        }
    }

    private void OnDestroy()
    {
        if (playerCoinCollector != null)
            playerCoinCollector.OnCoinsChanged.RemoveListener(UpdateCoinDisplay);
    }

    private void UpdateCoinDisplay(int coinAmount)
    {
        if (coinText != null)
            coinText.text = string.Format(textFormat, coinAmount);
    }
}
