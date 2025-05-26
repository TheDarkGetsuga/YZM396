using UnityEngine;

public class CoinInventory : MonoBehaviour
{
    public int coinCount = 0;

    private void Start()
    {
        coinCount = SaveManager.Instance.GetGold();
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        SaveManager.Instance.SetGold(coinCount);
        Debug.Log($"Added {amount} coins. Total coins: {coinCount}");
    }

    public int GetCoinCount()
    {
        return coinCount;
    }
}
