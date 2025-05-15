using UnityEngine;
using System.Collections.Generic;

public class PlayerKeyInventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();

    public void AddKey(string keyName)
    {
        if (keys.Add(keyName))
        {
            Debug.Log("Picked up key: [" + keyName + "]");
        }
        else
        {
            Debug.Log("Key already in inventory: [" + keyName + "]");
        }
    }

    public bool HasKey(string keyName)
    {
        return keys.Contains(keyName);
    }

    public bool RemoveKey(string keyName)
    {
        if (keys.Remove(keyName))
        {
            Debug.Log("Used and removed key: [" + keyName + "]");
            return true;
        }
        else
        {
            Debug.LogWarning("Tried to remove a key that doesn't exist: [" + keyName + "]");
            return false;
        }
    }

    public IEnumerable<string> GetAllKeys()
    {
        return keys;
    }
}
