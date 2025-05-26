using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string characterName;
    public string sceneName;
    public int gold;
    public List<string> obtainedSwordNames = new List<string>();
    public int playerDeaths = 0;
    public float totalPlayTime = 0f;
}