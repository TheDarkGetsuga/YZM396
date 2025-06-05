using UnityEngine;

public class CoinInventory : MonoBehaviour
{
    public int coinCount = 0;

    private void Start()
    {
        coinCount = SaveManager.Instance.GetGold(); // Set init count from SaveManager
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        SaveManager.Instance.SetGold(coinCount); //this might fail if its called too many times we gotta stress test it
        Debug.Log($"Added {amount} coins. Total coins: {coinCount}");
    }

    public int GetCoinCount()
    {
        return coinCount;
    }
}
