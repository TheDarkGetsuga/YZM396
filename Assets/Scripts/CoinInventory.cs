using UnityEngine;

public class CoinInventory : MonoBehaviour
{
    public int coinCount = 0; // The total number of coins the player has

    // Method to add coins
    public void AddCoins(int amount)
    {
        coinCount += amount;
        Debug.Log($"Added {amount} coins. Total coins: {coinCount}");
    }

    // Method to get the current number of coins
    public int GetCoinCount()
    {
        return coinCount;
    }
}
