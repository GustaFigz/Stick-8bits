using UnityEngine;
using UnityEngine.Events;

public class PlayerCoinCollector : MonoBehaviour
{
    [Header("Coins")]
    [SerializeField] private int currentCoins = 0;

    [Header("Events")]
    public UnityEvent<int> OnCoinsChanged;

    public int CurrentCoins => currentCoins;

    private void Start()
    {
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);

        Debug.Log($"Moedas coletadas: +{amount} | Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount)
            return false;

        currentCoins -= amount;
        OnCoinsChanged?.Invoke(currentCoins);
        return true;
    }

    public void ResetCoins()
    {
        currentCoins = 0;
        OnCoinsChanged?.Invoke(currentCoins);
    }
}
